using Altered.Core.Attributes;
using Altered.Core.Configure;
using System.Linq.Expressions;
using System.Reflection;

namespace Altered.Core.Main
{
    public static class DiffGenerator
    {
        private static TypeConfigurationManager? _typeConfiguratorManager;
        private static ComparerManager? _comparerManager;

        public static bool Ignore { get; set; } = false;
        public static bool Include { get; set; } = false;

        public static void Configure<TValue>() where TValue : class
        {
            _typeConfiguratorManager = new TypeConfigurationManager();

            var configurator = new TypeConfigurator();

            _typeConfiguratorManager.Configure<TValue>(configurator);
        }

        public static void Configure<TValue>(Action<TypeConfigurator> configure) where TValue : class
        {
            _typeConfiguratorManager = new TypeConfigurationManager();

            var configurator = new TypeConfigurator();

            configure.Invoke(configurator);

            _typeConfiguratorManager.Configure<TValue>(configurator);
        }

        public static void Configure<TValue>(TypeConfigurator configurator) where TValue : class
        {
            _typeConfiguratorManager = new TypeConfigurationManager();

            _typeConfiguratorManager.Configure<TValue>(configurator);
        }

        public static void RegisterComparer<TValue>(Func<TValue, TValue, bool> customComparer)
        {
            if (customComparer == null)
                throw new ArgumentNullException(nameof(customComparer));

            _comparerManager = new ComparerManager();

            _comparerManager.Register(customComparer);
        }

        public static List<DiffEntry> Generate<TValue>(TValue original, TValue modified, params Expression<Func<TValue, object>>[] propertySelectors)
            where TValue : class
        {
            if (original == null && modified == null) 
                return new List<DiffEntry>();
            
            if (original == null) 
                throw new ArgumentNullException(nameof(original));
            
            if (modified == null) 
                throw new ArgumentNullException(nameof(modified));

            var propertySelectorsProvided = propertySelectors.Any();
            if (propertySelectorsProvided)
            {
                if (_typeConfiguratorManager == null) 
                {
                    _typeConfiguratorManager = new TypeConfigurationManager();

                    _typeConfiguratorManager.Configure<TValue>();
                }

                if (!_typeConfiguratorManager.IsTypeConfigured<TValue>())
                {
                    _typeConfiguratorManager.Configure<TValue>();
                }
            }

            if (Ignore && !Include && propertySelectorsProvided)
                _typeConfiguratorManager.IgnoreProperties(propertySelectors);
            
            if (Include && !Ignore && propertySelectorsProvided)
                _typeConfiguratorManager.IncludeProperties(propertySelectors);

            var differentEntries = new List<DiffEntry>();
            var properties = typeof(TValue).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (prop.GetCustomAttribute<IgnoreInDiffAttribute>() != null)
                    continue;

                var propertyIsIgnored = _typeConfiguratorManager != null && _typeConfiguratorManager.IsTypeConfigured<TValue>() && _typeConfiguratorManager.PropertyIsIgnored<TValue>(prop.Name);
                if (propertyIsIgnored)
                    continue;

                var propertyIsNotIncluded = _typeConfiguratorManager != null && _typeConfiguratorManager.IsTypeConfigured<TValue>() && _typeConfiguratorManager.PropertyIsIncluded<TValue>(prop.Name);
                if (propertyIsNotIncluded)
                    continue;

                if (!prop.CanRead) continue;

                var oldValue = prop.GetValue(original);
                var newValue = prop.GetValue(modified);

                // Simple value comparison
                if (!AreEqual(oldValue, newValue))
                {
                    differentEntries.Add(new DiffEntry
                    {
                        PropertyName = prop.Name,
                        OldValue = oldValue,
                        NewValue = newValue
                    });
                }
            }

            return differentEntries;
        }

        public static void ClearAll()
        {
            if (_typeConfiguratorManager != null)
            {
                _typeConfiguratorManager.ClearAll();
            }
        }

        private static bool AreEqual(object original, object modified)
        {
            if (original == null && modified == null) return true;

            if (original == null || modified == null) return false;

            var type = original.GetType();
            if (_comparerManager != null && _comparerManager.IsRegistered(type))
            {
                var comparer = _comparerManager.Get(type);

                return (bool)comparer.DynamicInvoke(original, modified);
            }

            return original.Equals(modified);
        }
    }
}
