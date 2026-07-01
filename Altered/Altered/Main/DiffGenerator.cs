using Altered.Configure;
using System.Linq.Expressions;

namespace Altered.Main
{
    public static class DiffGenerator
    {
        private static Lazy<DiffEngine> _diffEngine = new(() => new DiffEngine());

        internal static DiffEngine DiffEngine => _diffEngine.Value;

        // -----------------------------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------------------------

        public static void Configure<TValue>()
            => DiffEngine.ConfigureType<TValue>();

        /// <summary>
        /// Configures a type using the specified configuration action.
        /// </summary>
        /// <typeparam name="TValue">The type to configure. Must be a reference type.</typeparam>
        /// <param name="configure">The action that defines the configuration for the type.</param>
        public static void Configure<TValue>(Action<TypeConfigurator> configure) 
            => DiffEngine.ConfigureTypeWithAction<TValue>(configure);

        /// <summary>
        /// Configures a type using the specified configurator.
        /// </summary>
        /// <typeparam name="TValue">The type to configure. Must be a reference type.</typeparam>
        /// <param name="configurator">The type configurator that defines the configuration for the type.</param>
        public static void Configure<TValue>(TypeConfigurator configurator) 
            => DiffEngine.ConfigureTypeWithConfigurator<TValue>(configurator);

        /// <summary>
        /// Registers a custom equality comparer for the specified value type.
        /// </summary>
        /// <typeparam name="TValue">The type of values to compare.</typeparam>
        /// <param name="customComparer">A function that determines whether two values of type TValue are equal.</param>
        public static void RegisterComparer<TValue>(Func<TValue, TValue, bool> customComparer)
            => DiffEngine.RegisterCustomComparer(customComparer);

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
            
            => DiffEngine.Generate(original, modified, propertySelectors, ignore);

        /// <summary>
        /// Generates a list of differences between two objects of the same type.
        /// </summary>
        /// <typeparam name="TValue">The type of the objects to compare. Must be a reference type.</typeparam>
        /// <param name="original">The original object to compare.</param>
        /// <param name="modified">The modified object to compare.</param>
        /// <returns>A list of differences between the original and modified objects.</returns>
        public static List<DiffEntry> Generate<TValue>(TValue original, TValue modified)   
            => DiffEngine.Generate(original, modified);

        /// <summary>
        /// Removes all configurations.
        /// </summary>
        public static void ClearAll()
            => DiffEngine.ClearAllConfigurations();
    }
}