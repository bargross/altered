using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Altered.Core.Configure
{
    internal class TypeConfigurationManager
    {
        private readonly ConcurrentDictionary<Type, TypeConfigurator> _ignoredPropertiesByType = new();

        public TypeConfigurationManager Configure<TValue>() where TValue : class => Configure(typeof(TValue));

        public TypeConfigurationManager Configure<TValue>(TypeConfigurator configurator) where TValue : class => Configure(typeof(TValue), configurator);

        public TypeConfigurationManager Configure(Type type, TypeConfigurator? configurator = null)
        {
            if (!_ignoredPropertiesByType.ContainsKey(type))
            {
                if (configurator != null)
                {
                    var update = _ignoredPropertiesByType.TryAdd(type, configurator);

                    if (!update)
                        throw new InvalidOperationException("Could not configure configurator for type");
                }
                else
                {
                    var typeConfigurator = new TypeConfigurator();

                    typeConfigurator.Configure(type);

                    var update = _ignoredPropertiesByType.TryAdd(type, typeConfigurator);

                    if (!update)
                        throw new InvalidOperationException("Could not configure configurator for type");
                }
            }

            return this;
        }

        public TypeConfigurationManager IgnoreProperties<TValue>(params Expression<Func<TValue, object>>[] propertySelectors) where TValue : class
        {
            var type = typeof(TValue);

            if (!_ignoredPropertiesByType.ContainsKey(type))
                throw new ArgumentException($"Type {type} was not configured, you must call Configure before IgnoreProperties.");

            var configurator = _ignoredPropertiesByType[type];

            configurator.IgnoreMany(propertySelectors);

            return this;
        }

        public TypeConfigurationManager IncludeProperties<TValue>(params Expression<Func<TValue, object>>[] propertySelectors) where TValue : class
        {
            var type = typeof(TValue);

            if (!_ignoredPropertiesByType.ContainsKey(type))
                throw new ArgumentException($"Type {type} was not configured, you must call Configure before IgnoreProperties.");

            var configurator = _ignoredPropertiesByType[type];

            configurator.IncludeMany(propertySelectors);

            return this;
        }

        public bool IsTypeConfigured<TValue>() => IsTypeConfigured(typeof(TValue));

        public bool IsTypeConfigured(Type type) => _ignoredPropertiesByType.ContainsKey(type);

        public bool PropertyIsIncluded<TValue>(string propertyName) => PropertyIsIncluded(typeof(TValue), propertyName);

        public bool PropertyIsIncluded(Type type, string propertyName)
        {
            Validate(type);

            return _ignoredPropertiesByType[type].IsIncluded(type, propertyName);
        }

        public bool PropertyIsIgnored<TValue>(string propertyName) => PropertyIsIgnored(typeof(TValue), propertyName);

        public bool PropertyIsIgnored(Type type, string propertyName)
        {
            Validate(type);

            return _ignoredPropertiesByType[type].IsIgnored(type, propertyName);
        }

        public void ClearAll() => _ignoredPropertiesByType.Clear();

        internal void Validate(Type type)
        {
            if (!_ignoredPropertiesByType.ContainsKey(type))
                throw new ArgumentException($"Type {type} was not configured, you must call Configure<T> before IgnoreProperties.");
        }
    }
}
