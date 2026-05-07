using Altered.Core.Attributes;
using System.Linq.Expressions;
using System.Reflection;

namespace Altered.Core.Main
{
    public static class DiffGenerator
    {
        private static IDictionary<string, List<string>> _propertiesToIgnore;

        private static void ConfigureProperties<TValue>(Expression<Func<TValue, object>>[] propertyIgnoreSelectors)
        {
            var propertyNames = new List<string>();
            var typeName = typeof(TValue).Name;

            foreach(var property in propertyIgnoreSelectors)
            {
                var propertyName = GetPropertyName(property);

                propertyNames.Add(propertyName);
            }

            ConfigureAndIgnore(typeName, propertyNames.ToArray());
        }

        public static List<DiffEntry> Generate<TValue>(TValue original, TValue modified, params Expression<Func<TValue, object>>[] propertyIgnoreSelectors)
            where TValue : class
        {
            if (original == null && modified == null) return new List<DiffEntry>();
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (modified == null) throw new ArgumentNullException(nameof(modified));

            if (propertyIgnoreSelectors.Any())
            {
                ConfigureProperties(propertyIgnoreSelectors);
            }

            var diffs = new List<DiffEntry>();
            var properties = typeof(TValue).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var typeName = typeof(TValue).Name;

            foreach (var prop in properties)
            {
                if (prop.GetCustomAttribute<IgnoreInDiffAttribute>() != null)
                    continue;

                if (_propertiesToIgnore != null && _propertiesToIgnore.ContainsKey(typeName) && _propertiesToIgnore[typeName].Contains(prop.Name))
                    continue;

                if (!prop.CanRead) continue;

                var oldValue = prop.GetValue(original);
                var newValue = prop.GetValue(modified);

                // Simple value comparison
                if (!AreEqual(oldValue, newValue))
                {
                    diffs.Add(new DiffEntry
                    {
                        PropertyName = prop.Name,
                        OldValue = oldValue,
                        NewValue = newValue
                    });
                }
            }

            return diffs;
        }

        public static void Configure<TValue>()
        {
            _propertiesToIgnore = new Dictionary<string, List<string>>();

            var genericTypeName = typeof(TValue).Name;

            _propertiesToIgnore.Add(genericTypeName, new List<string>());
        }

        public static void ConfigureAndIgnore(string typeName, params string[] properties)
        {
            if (_propertiesToIgnore == null)
            {
                _propertiesToIgnore = new Dictionary<string, List<string>>
                {
                    { typeName, properties.ToList() }
                };
            }
            else
            {
                _propertiesToIgnore[typeName].AddRange(properties);
            }
        }

        public static void Ignore<TValue>(Expression<Func<TValue, object>> propertyIgnoreSelector)
        {
            if (_propertiesToIgnore == null)
            {
                throw new ArgumentNullException(nameof(propertyIgnoreSelector));
            }

            var typeName = typeof(TValue).Name;
            if (!_propertiesToIgnore.ContainsKey(typeof(TValue).Name))
            {
                throw new ArgumentException($"Type {typeName} has not been configured, call Configure<T> before ignore");
            }

            var propertyName = GetPropertyName(propertyIgnoreSelector);

            _propertiesToIgnore[typeName].Add(propertyName);
        }

        private static string GetPropertyName<TValue>(Expression<Func<TValue, object>> expression)
        {
            switch (expression.Body)
            {
                case MemberExpression memberExpression:
                    return memberExpression.Member.Name;

                case UnaryExpression unaryExpression when unaryExpression.Operand is MemberExpression memberExpression:
                    return memberExpression.Member.Name;

                default:
                    throw new ArgumentException($"Expression '{expression}' does not refer to a property");
            }
        }

        private static bool AreEqual(object original, object modified)
        {
            if (original == null && modified == null) return true;

            if (original == null || modified == null) return false;

            return original.Equals(modified);
        }
    }
}
