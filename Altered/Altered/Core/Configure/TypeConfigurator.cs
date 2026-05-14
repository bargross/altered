using Altered.Core.Extensions;
using System.Linq.Expressions;

namespace Altered.Core.Configure
{
    public class TypeConfigurator: ITypeConfigurator
    {
        private readonly HashSet<string> _propertiesToIgnore;
        private readonly HashSet<string> _propertiesToInclude;
        private Type? _type;

        private bool _isInclusion = false;
        private bool _isExclusion = false;

        public Type Type
        {
            get
            {
                if (_type == null)
                {
                    throw new ArgumentNullException("type");
                }

                return _type;
            }
        }

        public TypeConfigurator()
        {
            _propertiesToIgnore = new HashSet<string>();
            _propertiesToInclude = new HashSet<string>();
        }

        public TypeConfigurator(Type type)
        {
            _type = type;

            _propertiesToIgnore = new HashSet<string>();
            _propertiesToInclude = new HashSet<string>();
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

        public bool IsIgnored<TValue>(string propertyName) => IsIgnored(typeof(TValue), propertyName);

        public bool IsIgnored(Type type, string propertyName) => type == _type ? _propertiesToIgnore.Contains(propertyName) : false;

        public bool IsIncluded<TValue>(string propertyName) => IsIncluded(typeof(TValue), propertyName);

        public bool IsIncluded(Type type, string propertyName) => type == _type ? _propertiesToInclude.Contains(propertyName) : false;

        public ITypeConfigurator Ignore<TValue>(Expression<Func<TValue, object>> propertyIgnoreSelector)
        {
            ValidateConfigurationType(true, false);

            _propertiesToIgnore.Add(propertyIgnoreSelector.GetPropertyName());

            return this;
        }

        public ITypeConfigurator IgnoreMany<TValue>(params Expression<Func<TValue, object>>[] propertyIgnoreSelectors)
        {
            ValidateConfigurationType(true, false);

            return AddToCollection(_propertiesToIgnore, propertyIgnoreSelectors);
        }

        public ITypeConfigurator Include<TValue>(Expression<Func<TValue, object>> propertyIgnoreSelector)
        {
            ValidateConfigurationType(false, true);

            _propertiesToInclude.Add(propertyIgnoreSelector.GetPropertyName());

            return this;
        }

        public ITypeConfigurator IncludeMany<TValue>(params Expression<Func<TValue, object>>[] propertyIncludeSelectors)
        {
            ValidateConfigurationType(false, true);

            return AddToCollection(_propertiesToInclude, propertyIncludeSelectors);
        }

        public void Clear() => _propertiesToIgnore.Clear();

        public IReadOnlySet<string> GetIgnoredProperties() => _propertiesToIgnore;


        public IReadOnlySet<string> GetIncludedProperties() => _propertiesToInclude;

        private void ValidateConfigurationType(bool exclusion, bool inclusion)
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

        private ITypeConfigurator AddToCollection<TValue>(HashSet<string> collection, params Expression<Func<TValue, object>>[] propertyIgnoreSelectors)
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
