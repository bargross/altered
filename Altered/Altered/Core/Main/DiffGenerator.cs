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

        // -----------------------------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------------------------

        public static void Configure<TValue>() where TValue : class
            => ConfigureType<TValue>();

        /// <summary>
        /// Configures a type using the specified configuration action.
        /// </summary>
        /// <typeparam name="TValue">The type to configure. Must be a reference type.</typeparam>
        /// <param name="configure">The action that defines the configuration for the type.</param>
        public static void Configure<TValue>(Action<TypeConfigurator> configure) where TValue : class
            => ConfigureTypeWithAction<TValue>(configure);

        /// <summary>
        /// Configures a type using the specified configurator.
        /// </summary>
        /// <typeparam name="TValue">The type to configure. Must be a reference type.</typeparam>
        /// <param name="configurator">The type configurator that defines the configuration for the type.</param>
        public static void Configure<TValue>(TypeConfigurator configurator) where TValue : class
            => ConfigureTypeWithConfigurator<TValue>(configurator);

        /// <summary>
        /// Registers a custom equality comparer for the specified value type.
        /// </summary>
        /// <typeparam name="TValue">The type of values to compare.</typeparam>
        /// <param name="customComparer">A function that determines whether two values of type TValue are equal.</param>
        public static void RegisterComparer<TValue>(Func<TValue, TValue, bool> customComparer)
            => RegisterCustomComparer(customComparer);

        /// <summary>
        /// Compares two objects and returns a list of differences for the specified properties.
        /// </summary>
        /// <typeparam name="TValue">The type of the objects to compare.</typeparam>
        /// <param name="original">The original object.</param>
        /// <param name="modified">The modified object to compare against the original.</param>
        /// <param name="ignore">Indicates whether to ignore properties or include if false.</param>
        /// <param name="propertySelectors">Expressions specifying the properties to include in the comparison.</param>
        /// <returns>A list of differences between the original and modified objects.</returns>
        public static List<DiffEntry> Generate<TValue>(TValue original, TValue modified, bool ignore, params Expression<Func<TValue, object>>[] propertySelectors)
            where TValue : class
            => GenerateDiffs(original, modified, propertySelectors, ignore);

        /// <summary>
        /// Generates a list of differences between two objects of the same type.
        /// </summary>
        /// <typeparam name="TValue">The type of the objects to compare. Must be a reference type.</typeparam>
        /// <param name="original">The original object to compare.</param>
        /// <param name="modified">The modified object to compare.</param>
        /// <returns>A list of differences between the original and modified objects.</returns>
        public static List<DiffEntry> Generate<TValue>(TValue original, TValue modified) where TValue : class  
            => GenerateDiffs(original, modified, []);

        /// <summary>
        /// Removes all configurations.
        /// </summary>
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
            if (configurator is null)
                throw new ArgumentNullException(nameof(configurator));

            _typeConfiguratorManager.Configure<TValue>(configurator);
        }

        internal static void RegisterCustomComparer<TValue>(Func<TValue, TValue, bool> customComparer)
        {
            if (customComparer is null)
                throw new ArgumentNullException(nameof(customComparer));

            _comparerManager.Register(customComparer);
        }

        internal static List<DiffEntry> GenerateDiffs<TValue>(TValue original, TValue modified, Expression<Func<TValue, object>>[] propertySelectors, bool? ignore = null)
            where TValue : class
        {
            if (original is null && modified is null)
                return new List<DiffEntry>();

            if (original is null)
                throw new ArgumentNullException(nameof(original));

            if (modified is null)
                throw new ArgumentNullException(nameof(modified));

            if (ignore is null && propertySelectors.Any())
                throw new InvalidOperationException("Must specify whether to ignore or include properties.");

            if (ignore is true)
                _typeConfiguratorManager.BlackList(typeof(TValue), true);

            if (ignore is false)
                _typeConfiguratorManager.WhiteList(typeof(TValue), true);

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
            if (!selectorsProvided)
                return;

            if (!_typeConfiguratorManager.IsTypeConfigured<TValue>())
                _typeConfiguratorManager.Configure<TValue>();

            if (_typeConfiguratorManager.IsUsingIgnore<TValue>())
                _typeConfiguratorManager.IgnoreProperties(propertySelectors);

            if (_typeConfiguratorManager.IsUsingInclude<TValue>())
                _typeConfiguratorManager.IncludeProperties(propertySelectors);
        }

        internal static bool ShouldSkipProperty<TValue>(PropertyInfo prop)
        {
            if (!prop.CanRead)
                return true;

            if (prop.GetCustomAttribute<IgnoreInDiffAttribute>() is not null)
                return true;

            if (!_typeConfiguratorManager.IsTypeConfigured<TValue>())
                return false;

            if (_typeConfiguratorManager.IsUsingIgnore<TValue>())
                return _typeConfiguratorManager.PropertyIsIgnored<TValue>(prop.Name);

            if (_typeConfiguratorManager.IsUsingInclude<TValue>())
                return !_typeConfiguratorManager.PropertyIsIncluded<TValue>(prop.Name);

            return false;
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
            if (original is null && modified is null)
                return true;

            if (original is null || modified is null)
                return false;

            var type = original.GetType();

            if (_comparerManager != null && _comparerManager.IsRegistered(type))
                return InvokeCustomComparer(type, original, modified);

            return original.Equals(modified);
        }

        internal static bool InvokeCustomComparer(Type type, object original, object modified)
        {
            var comparer = _comparerManager.Get(type);

            var result = comparer.DynamicInvoke(original, modified);

            if (result is bool boolResult)
                return boolResult;

            throw new InvalidOperationException($"Comparer for type {type.FullName} did not return a boolean value.");
        }
    }
}