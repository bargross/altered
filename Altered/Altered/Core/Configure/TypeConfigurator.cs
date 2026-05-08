using Altered.Core.Extensions;
using System.Linq.Expressions;

namespace Altered.Core.Configure
{
    public class TypeConfigurator: ITypeConfigurator
    {
        private readonly HashSet<string> _propertiesToIgnore;
        private Type? _type;

        public TypeConfigurator()
        {
            _propertiesToIgnore = new HashSet<string>();
        }

        public TypeConfigurator(Type type)
        {
            _type = type;

            _propertiesToIgnore = new HashSet<string>();
        }

        public bool IsIgnored<TValue>(string propertyName)
        {
            var type = typeof(TValue);

            return IsIgnored(type, propertyName);
        }

        public bool IsIgnored(Type type, string propertyName)
        {
            if (_type != type)
                throw new ArgumentException($"Configurator is not for type {type}");

            return _propertiesToIgnore.Contains(propertyName);
        }

        public ITypeConfigurator Configure<TValue>()
        {
            Configure(typeof(TValue));

            return this;
        }

        public ITypeConfigurator Configure(Type type)
        {
            _type = type;

            return this;
        }

        public ITypeConfigurator Ignore<TValue>(Expression<Func<TValue, object>> propertyIgnoreSelector)
        {
            _propertiesToIgnore.Add(propertyIgnoreSelector.GetPropertyName());

            return this;
        }

        public ITypeConfigurator IgnoreMany<TValue>(params Expression<Func<TValue, object>>[] propertyIgnoreSelectors)
        {
            var propertyNames = new List<string>();
            var type = typeof(TValue);

            foreach (var property in propertyIgnoreSelectors)
            {
                var propertyName = property.GetPropertyName();

                propertyNames.Add(propertyName);
            }

            propertyNames.ForEach(name =>
            {
                _propertiesToIgnore.Add(name);
            });

            return this;
        }
    }
}
