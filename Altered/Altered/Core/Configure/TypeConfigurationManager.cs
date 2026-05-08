using System.Linq.Expressions;

namespace Altered.Core.Configure
{
    internal class TypeConfigurationManager : ITypeConfigurationManager
    {
        private readonly IDictionary<Type, TypeConfigurator> _ignoredPropertiesByType;

        public TypeConfigurationManager()
        {
            _ignoredPropertiesByType = new Dictionary<Type, TypeConfigurator>();
        }

        public ITypeConfigurationManager Configure<TValue>() where TValue : class
        {
            return Configure(typeof(TValue));
        }

        public ITypeConfigurationManager Configure<TValue>(TypeConfigurator configurator) where TValue : class
        {
            return Configure(typeof(TValue), configurator);
        }

        public ITypeConfigurationManager Configure(Type type, TypeConfigurator? configurator = null)
        {
            if (!_ignoredPropertiesByType.ContainsKey(type))
            {
                if (configurator != null)
                {
                    _ignoredPropertiesByType.Add(type, configurator);   
                }
                else
                {
                    var typeConfigurator = new TypeConfigurator();

                    typeConfigurator.Configure(type);

                    _ignoredPropertiesByType.Add(type, typeConfigurator);
                }
            }

            return this;
        }

        public ITypeConfigurationManager IgnoreProperties<TValue>(params Expression<Func<TValue, object>>[] propertySelectors) where TValue : class
        {
            var type = typeof(TValue);

            if (!_ignoredPropertiesByType.ContainsKey(type))
                throw new ArgumentException($"Type {type} was not configured, you must call Configure<T> before IgnoreProperties.");

            var configurator = _ignoredPropertiesByType[type];

            configurator.IgnoreMany(propertySelectors);

            return this;
        }

        public bool IsTypeConfigured(Type type)
        {
            Validate(type);

            return _ignoredPropertiesByType.ContainsKey(type);
        }

        public bool IsTypeConfigured<TValue>()
        {
            var type = typeof(TValue);

            Validate(type);

            return _ignoredPropertiesByType.ContainsKey(type);
        }

        public bool PropertyIsIgnored<TValue>(string propertyName)
        {
            var type = typeof(TValue);

            return PropertyIsIgnored(type, propertyName);
        }

        public bool PropertyIsIgnored(Type type, string propertyName)
        {
            Validate(type);

            return _ignoredPropertiesByType[type].IsIgnored(type, propertyName);
        }

        private void Validate(Type type)
        {
            if (!_ignoredPropertiesByType.ContainsKey(type))
                throw new ArgumentException($"Type {type} was not configured, you must call Configure<T> before IgnoreProperties.");
        }
    }
}
