using Altered.Core.Attributes;
using Altered.Core.Configure;
using System.Linq.Expressions;
using System.Reflection;

namespace Altered.Core.Main
{
    public static class DiffGenerator
    {
        internal static TypeConfigurationManager _typeConfiguratorManager = new();
        internal static ComparerManager _comparerManager = new();

        public static bool Ignore { get; set; } = false;
        public static bool Include { get; set; } = false;

        // -----------------------------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------------------------

        public static void Configure<TValue>() where TValue : class
            => ConfigureType<TValue>();

        public static void Configure<TValue>(Action<TypeConfigurator> configure) where TValue : class
            => ConfigureTypeWithAction<TValue>(configure);

        public static void Configure<TValue>(TypeConfigurator configurator) where TValue : class
            => ConfigureTypeWithConfigurator<TValue>(configurator);

        public static void RegisterComparer<TValue>(Func<TValue, TValue, bool> customComparer)
            => RegisterCustomComparer(customComparer);

        public static List<DiffEntry> Generate<TValue>(TValue original, TValue modified, params Expression<Func<TValue, object>>[] propertySelectors)
            where TValue : class
            => GenerateDiffs(original, modified, propertySelectors);

        public static void ClearAll()
            => ClearAllConfigurations();

        // -----------------------------------------------------------------------------------------
        // Internal implementation
        // -----------------------------------------------------------------------------------------

        internal static void ConfigureType<TValue>() where TValue : class
        {
            _typeConfiguratorManager = new TypeConfigurationManager();
            var configurator = new TypeConfigurator();
            _typeConfiguratorManager.Configure<TValue>(configurator);
        }

        internal static void ConfigureTypeWithAction<TValue>(Action<TypeConfigurator> configure) where TValue : class
        {
            _typeConfiguratorManager = new TypeConfigurationManager();
            var configurator = new TypeConfigurator();
            configure.Invoke(configurator);
            _typeConfiguratorManager.Configure<TValue>(configurator);
        }

        internal static void ConfigureTypeWithConfigurator<TValue>(TypeConfigurator configurator) where TValue : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            _typeConfiguratorManager.Configure<TValue>(configurator);
        }

        internal static void RegisterCustomComparer<TValue>(Func<TValue, TValue, bool> customComparer)
        {
            if (customComparer == null)
                throw new ArgumentNullException(nameof(customComparer));

            _comparerManager.Register(customComparer);
        }

        internal static List<DiffEntry> GenerateDiffs<TValue>(TValue original, TValue modified, Expression<Func<TValue, object>>[] propertySelectors)
            where TValue : class
        {
            if (original == null && modified == null)
                return new List<DiffEntry>();

            if (original == null)
                throw new ArgumentNullException(nameof(original));

            if (modified == null)
                throw new ArgumentNullException(nameof(modified));

            ApplyPropertySelectors(propertySelectors);

            var differentEntries = new List<DiffEntry>();
            var properties = typeof(TValue).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (ShouldSkipProperty<TValue>(prop))
                    continue;

                if (TryBuildDiffEntry(prop, original, modified, out var entry))
                    differentEntries.Add(entry!);
            }

            return differentEntries;
        }

        internal static void ApplyPropertySelectors<TValue>(Expression<Func<TValue, object>>[] propertySelectors)
            where TValue : class
        {
            var selectorsProvided = propertySelectors.Any();

            if (selectorsProvided && !_typeConfiguratorManager.IsTypeConfigured<TValue>())
                _typeConfiguratorManager.Configure<TValue>();

            if (Ignore && !Include && selectorsProvided)
                _typeConfiguratorManager.IgnoreProperties(propertySelectors);

            if (Include && !Ignore && selectorsProvided)
                _typeConfiguratorManager.IncludeProperties(propertySelectors);
        }

        internal static bool ShouldSkipProperty<TValue>(PropertyInfo prop)
        {
            if (!prop.CanRead)
                return true;

            if (prop.GetCustomAttribute<IgnoreInDiffAttribute>() != null)
                return true;

            if (!_typeConfiguratorManager.IsTypeConfigured<TValue>())
                return false;

            return _typeConfiguratorManager.PropertyIsIgnored<TValue>(prop.Name)
                || _typeConfiguratorManager.PropertyIsIncluded<TValue>(prop.Name);
        }

        internal static bool TryBuildDiffEntry<TValue>(PropertyInfo prop, TValue original, TValue modified, out DiffEntry? entry)
        {
            var oldValue = prop.GetValue(original);
            var newValue = prop.GetValue(modified);

            if (AreEqual(oldValue, newValue))
            {
                entry = null;
                return false;
            }

            entry = new DiffEntry
            {
                PropertyName = prop.Name,
                OldValue = oldValue,
                NewValue = newValue
            };

            return true;
        }

        internal static void ClearAllConfigurations()
        {
            _typeConfiguratorManager?.ClearAll();
        }

        internal static bool AreEqual(object? original, object? modified)
        {
            if (original == null && modified == null)
                return true;

            if (original == null || modified == null)
                return false;

            var type = original.GetType();

            if (_comparerManager != null && _comparerManager.IsRegistered(type))
                return InvokeCustomComparer(type, original, modified);

            return original.Equals(modified);
        }

        internal static bool InvokeCustomComparer(Type type, object original, object modified)
        {
            var comparer = _comparerManager.Get(type)
                ?? throw new InvalidOperationException($"No comparer registered for type {type.FullName}");

            var result = comparer.DynamicInvoke(original, modified);

            if (result is bool boolResult)
                return boolResult;

            throw new InvalidOperationException($"Comparer for type {type.FullName} did not return a boolean value.");
        }
    }
}