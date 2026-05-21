using Altered.Core.Extensions;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace Altered.Core.Configure
{
    public class TypeConfigurator: ITypeConfigurator
    {
        internal readonly ConcurrentBag<string> _propertiesToIgnore = new();
        internal readonly ConcurrentBag<string> _propertiesToInclude = new();
        internal Type? _type;

        internal bool _isInclusion = false;
        internal bool _isExclusion = false;

        internal IReadOnlySet<string> IngnoredPropertiesSet => _propertiesToIgnore.ToHashSet();
        internal IReadOnlySet<string> IncludedPropertiesSet => _propertiesToInclude.ToHashSet();

        //=========================================================================================================================
        //                                                  Public Methods
        //=========================================================================================================================


        /// <summary>
        /// Gets the configured type.
        /// </summary>
        public Type Type => GetConfiguredType();

        /// <summary>
        /// Gets the set of property names to ignore during configuration.
        /// </summary>
        public IReadOnlySet<string> IgnoredProperties => IngnoredPropertiesSet;

        /// <summary>
        /// Gets the collection of included property names.
        /// </summary>
        public IReadOnlySet<string> IncludedProperties => IncludedPropertiesSet;

        /// <summary>
        /// Configures the specified value type based on the specific generic TValue.
        /// </summary>
        /// <typeparam name="TValue">The type to configure.</typeparam>
        /// <returns>An instance of ITypeConfigurator for further configuration.</returns>
        public ITypeConfigurator Configure<TValue>() => ConfigureType(typeof(TValue));

        /// <summary>
        /// Configures the specified value type by providing the type.
        /// </summary>
        /// <typeparam name="TValue">The type to configure.</typeparam>
        /// <returns>An instance of ITypeConfigurator for further configuration.</returns>
        public ITypeConfigurator Configure(Type type) => ConfigureType(type);

        /// <summary>
        /// Determines whether the specified property is ignored for the given generic type.
        /// </summary>
        /// <typeparam name="TValue">The type that contains the property.</typeparam>
        /// <param name="propertyName">The name of the property to check.</param>
        /// <returns>true if the property is ignored; otherwise, false.</returns>
        public bool IsIgnored<TValue>(string propertyName) => IsPropertyIgnored(typeof(TValue), propertyName);

        /// <summary>
        /// Determines whether the specified property is ignored for the given type.
        /// </summary>
        /// <typeparam name="TValue">The type that contains the property.</typeparam>
        /// <param name="propertyName">The name of the property to check.</param>
        /// <returns>true if the property is ignored; otherwise, false.</returns>
        public bool IsIgnored(Type type, string propertyName) => IsPropertyIgnored(type, propertyName);

        /// <summary>
        /// Determines whether the specified property is included for the given generic type.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool IsIncluded<TValue>(string propertyName) => IsPropertyIncluded(typeof(TValue), propertyName);

        /// <summary>
        /// Determines whether the specified property of a type is included.
        /// </summary>
        /// <param name="type">The type to inspect for the property.</param>
        /// <param name="propertyName">The name of the property to check.</param>
        /// <returns>true if the property is included; otherwise, false.</returns>
        public bool IsIncluded(Type type, string propertyName) => IsPropertyIncluded(type, propertyName);

        /// <summary>
        /// Excludes the specified property from configuration based on the provided generic type.
        /// </summary>
        /// <typeparam name="TValue">The type containing the property to exclude.</typeparam>
        /// <param name="propertyIgnoreSelector">An expression selecting the property to exclude.</param>
        /// <returns>An ITypeConfigurator instance for further configuration.</returns>
        public ITypeConfigurator Ignore<TValue>(Expression<Func<TValue, object>> propertyIgnoreSelector) => IgnoreManyProperties(propertyIgnoreSelector);

        /// <summary>
        /// Ignores multiple properties for the specified type using the provided expressions.
        /// </summary>
        /// <typeparam name="TValue">The type containing the properties to ignore.</typeparam>
        /// <param name="propertyIgnoreSelectors">Expressions that specify the properties to ignore.</param>
        /// <returns>An ITypeConfigurator instance for further configuration.</returns>
        public ITypeConfigurator IgnoreMany<TValue>(params Expression<Func<TValue, object>>[] propertyIgnoreSelectors) => IgnoreManyProperties(propertyIgnoreSelectors);

        /// <summary>
        /// Includes the specified properties for configuration.
        /// </summary>
        /// <typeparam name="TValue">The type of the object containing the properties to include.</typeparam>
        /// <param name="propertyIgnoreSelector">An expression selecting the properties to include.</param>
        /// <returns>An instance of ITypeConfigurator for further configuration.</returns>
        public ITypeConfigurator Include<TValue>(Expression<Func<TValue, object>> propertyIgnoreSelector) => IncludeManyProperties(propertyIgnoreSelector);

        /// <summary>
        /// Includes multiple properties for configuration based on the specified selectors.
        /// </summary>
        /// <typeparam name="TValue">The type of the values being configured.</typeparam>
        /// <param name="propertyIncludeSelectors">An array of expressions that specify the properties to include.</param>
        /// <returns>An instance of ITypeConfigurator for further configuration.</returns>
        public ITypeConfigurator IncludeMany<TValue>(params Expression<Func<TValue, object>>[] propertyIncludeSelectors) => IncludeManyProperties(propertyIncludeSelectors);

        /// <summary>
        /// Removes all properties from the current instance.
        /// </summary>
        public void Clear() => ClearAllProperties();


        //=========================================================================================================================
        //                                                  Internal Methods
        //=========================================================================================================================

        internal Type GetConfiguredType()
        {
            if (_type == null)
            {
                throw new ArgumentNullException("type");
            }

            return _type;
        }

        internal void ClearAllProperties() => _propertiesToIgnore.Clear();

        internal bool IsPropertyIgnored(Type type, string propertyName) => type == _type ? _propertiesToIgnore.Contains(propertyName) : false;

        internal bool IsPropertyIncluded(Type type, string propertyName) => type == _type ? _propertiesToInclude.Contains(propertyName) : false;

        internal ITypeConfigurator IgnoreManyProperties<TValue>(params Expression<Func<TValue, object>>[] propertyIgnoreSelectors)
        {
            ValidateConfigurationType(true, false);

            AddToCollection(_propertiesToIgnore, propertyIgnoreSelectors);

            return this;
        }

        internal ITypeConfigurator IncludeManyProperties<TValue>(params Expression<Func<TValue, object>>[] propertyIncludeSelectors)
        {
            ValidateConfigurationType(false, true);

            AddToCollection(_propertiesToInclude, propertyIncludeSelectors);
            
            return this;
        }

        internal ITypeConfigurator ConfigureType(Type type)
        {
            _type = type;

            return this;
        }

        internal void ValidateConfigurationType(bool exclusion, bool inclusion)
        {
            if (!_isInclusion && !_isExclusion)
            {
                _isExclusion = exclusion;
                _isInclusion = inclusion;
            }

            if (_isInclusion && exclusion)
                throw new ArgumentException("Cannot ignore when already including properties.");

            if (_isExclusion && inclusion)
                throw new ArgumentException("Cannot include when ignoring properties.");
        }

        internal ITypeConfigurator AddToCollection<TValue>(ConcurrentBag<string> collection, params Expression<Func<TValue, object>>[] propertyIgnoreSelectors)
        {
            var propertyNames = new List<string>();
            var type = typeof(TValue);

            foreach (var property in propertyIgnoreSelectors)
            {
                var propertyName = property.GetPropertyName();

                collection.Add(propertyName);
            }

            return this;
        }
    }
}
