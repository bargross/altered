using Altered.Extensions;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace Altered.Configure
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
        //                                                  Public Constructors
        //=========================================================================================================================

        public TypeConfigurator() { }

        public TypeConfigurator(Type type)
        {
            Configure(type);
        }

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
        /// ensure internal usage of properties black list properties (ignore), calling Ignore sets it as well, this is optional.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ITypeConfigurator BlackList(bool value) => BlackListProperties(value);

        /// <summary>
        /// ensure internal usage of properties white lists properties (include), calling Include sets it as well, this is optional.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ITypeConfigurator WhiteList(bool value) => WhiteListProperties(value);

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

        internal void ClearAllProperties() 
        {
            _isExclusion = false;
            _isInclusion = false;
            _propertiesToIgnore.Clear();
            _propertiesToInclude.Clear();
        }

        internal bool IsPropertyIgnored(Type type, string propertyName) => type == _type ? _propertiesToIgnore.Contains(propertyName) : false;

        internal bool IsPropertyIncluded(Type type, string propertyName) => type == _type ? _propertiesToInclude.Contains(propertyName) : false;

        internal ITypeConfigurator IgnoreManyProperties<TValue>(params Expression<Func<TValue, object>>[] propertyIgnoreSelectors)
        {
            var type = typeof(TValue);
            if (_type == null)
                _type = type;
            
            if (_type != type) 
                throw new ArgumentException($"Type given does not match configured type {_type.Name}");

            BlackListProperties(true);

            AddToCollection(_propertiesToIgnore, propertyIgnoreSelectors);

            return this;
        }

        internal ITypeConfigurator IncludeManyProperties<TValue>(params Expression<Func<TValue, object>>[] propertyIncludeSelectors)
        {
            var type = typeof(TValue);
            if (_type == null)
                _type = type;

            if (_type != type)
                throw new ArgumentException($"Type given does not match configured type {_type.Name}");

            WhiteListProperties(true);

            AddToCollection(_propertiesToInclude, propertyIncludeSelectors);
            
            return this;
        }

        internal ITypeConfigurator ConfigureType(Type type)
        {
            if (_type == null)
                _type = type;

            else if (_type == type) return this;

            else throw new InvalidCastException($"Type already configured for type {_type.Name}");

            return this;
        }

        internal ITypeConfigurator BlackListProperties(bool value)
        {
            if (value == _isExclusion && _isExclusion != _isInclusion)
                return this;

            if (value && _isInclusion)
                throw new InvalidOperationException("Cannot blacklist when already using whitelist mode");

            _isExclusion = value;

            return this;
        }

        internal ITypeConfigurator WhiteListProperties(bool value)
        {
            if (value == _isInclusion && _isInclusion !=  _isExclusion)
                return this;

            if (value && _isExclusion)
                throw new InvalidOperationException("Cannot whitelist when already using blacklist mode");

            _isInclusion = value;

            return this;
        }

        internal ITypeConfigurator AddToCollection<TValue>(ConcurrentBag<string> collection, params Expression<Func<TValue, object>>[] propertyIgnoreSelectors)
        {
            if (!propertyIgnoreSelectors.Any()) return this;

            foreach (var property in propertyIgnoreSelectors)
            {
                var propertyName = property.GetPropertyName();

                // if property does not exist on the type, add it, if not we ignore it.
                if (!collection.Contains(propertyName))
                    collection.Add(propertyName);
            }

            return this;
        }
    }
}
