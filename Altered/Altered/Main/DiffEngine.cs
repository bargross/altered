using Altered.Attributes;
using Altered.Configure;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Altered.Main
{
    internal class DiffEngine
    {
        internal readonly TypeConfigurationManager _typeConfiguratorManager = new();
        internal readonly ComparerManager _comparerManager = new();
        internal readonly ConcurrentDictionary<Type, Func<object, object, bool>> _comparerWrappers = new();

        internal void ConfigureType<TValue>()
        {
            var configurator = new TypeConfigurator();

            _typeConfiguratorManager.Configure<TValue>(configurator);
        }

        internal void ConfigureTypeWithAction<TValue>(Action<TypeConfigurator> configure)
        {
            if (configure is null)
                throw new ArgumentNullException(nameof(configure));

            var configurator = new TypeConfigurator();

            configure.Invoke(configurator);

            _typeConfiguratorManager.Configure<TValue>(configurator);
        }

        internal void ConfigureTypeWithConfigurator<TValue>(TypeConfigurator configurator)
        {
            if (configurator is null)
                throw new ArgumentNullException(nameof(configurator));

            _typeConfiguratorManager.Configure<TValue>(configurator);
        }

        internal void RegisterCustomComparer<TValue>(Func<TValue, TValue, bool> customComparer)
        {
            if (customComparer is null)
                throw new ArgumentNullException(nameof(customComparer));

            _comparerManager.Register(customComparer);
        }

        internal List<DiffEntry> Generate<TValue>(TValue original, TValue modified, Expression<Func<TValue, object>>[]? propertySelectors = null, bool? ignore = null)
        {
            if (!_typeConfiguratorManager.IsTypeConfigured<TValue>())
                _typeConfiguratorManager.Configure<TValue>();

            if (original is null && modified is null)
                return new List<DiffEntry>();

            if (original is null)
                throw new ArgumentNullException(nameof(original));

            if (modified is null)
                throw new ArgumentNullException(nameof(modified));

            if (ignore is null && propertySelectors?.Any() == true)
                throw new InvalidOperationException("Must specify whether to ignore or include properties.");

            if (ignore is true && _typeConfiguratorManager is not null)
                _typeConfiguratorManager.BlackList(typeof(TValue), true);

            if (ignore is false && _typeConfiguratorManager is not null)
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

        internal void ApplyPropertySelectors<TValue>(Expression<Func<TValue, object>>[] propertySelectors)
        {
            if (propertySelectors is null || !propertySelectors.Any())
                return;

            if (!_typeConfiguratorManager.IsTypeConfigured<TValue>())
                _typeConfiguratorManager.Configure<TValue>();

            if (_typeConfiguratorManager.IsUsingIgnore<TValue>())
                _typeConfiguratorManager.IgnoreProperties(propertySelectors);

            if (_typeConfiguratorManager.IsUsingInclude<TValue>())
                _typeConfiguratorManager.IncludeProperties(propertySelectors);
        }

        internal bool ShouldSkipProperty<TValue>(PropertyInfo prop)
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

        internal bool TryBuildDiffEntry<TValue>(PropertyInfo prop, TValue original, TValue modified, out DiffEntry? entry)
        {
            var oldValue = prop.GetValue(original);
            var newValue = prop.GetValue(modified);

            if (AreValuesEqual(oldValue, newValue))
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

        internal void ClearAllConfigurations()
        {
            _comparerManager?.Clear();
            _typeConfiguratorManager?.ClearAll();
        }

        internal bool AreValuesEqual(object? original, object? modified, HashSet<object>? visited = null)
        {
            visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);

            if (original == null && modified == null) return true;

            if (original == null || modified == null) return false;

            var type = original.GetType();

            if (_comparerManager.IsRegistered(type))
                return InvokeCustomComparer(type, original, modified);

            if (IsSimpleType(type))
                return Equals(original, modified);

            if (visited.Contains(original)) return true;
                visited.Add(original);

            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string) && type != typeof(byte[]))
                return CompareCollections((IEnumerable)original, (IEnumerable)modified, visited);

            // Complex objects – use diff engine recursively
            return CompareComplex(original, modified, visited);
        }

        internal static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type.IsEnum ||
                   type == typeof(decimal) || type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) || type == typeof(Guid) ||
                   type == typeof(byte[]);
        }

        internal bool CompareCollections(IEnumerable seqA, IEnumerable seqB, HashSet<object> visited)
        {
            var listA = seqA.Cast<object>().ToList();
            var listB = seqB.Cast<object>().ToList();
            if (listA.Count != listB.Count) return false;

            for (int i = 0; i < listA.Count; i++)
            {
                if (!AreValuesEqual(listA[i], listB[i], visited))
                    return false;
            }

            return true;
        }

        internal bool CompareComplex(object original, object modified, HashSet<object> visited)
        {
            var type = original.GetType();
            // Get the generic Generate method
            var method = typeof(DiffEngine)
                .GetMethod(nameof(Generate), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                ?.MakeGenericMethod(type);

            if (method == null)
                throw new InvalidOperationException($"Generate method not found for type {type}");

            var diffs = (IEnumerable<DiffEntry>)method.Invoke(this, new object[] { original, modified, null, null });

            return !diffs.Any();
        }

        internal bool InvokeCustomComparer(Type type, object original, object modified)
        {
            var wrapper = _comparerWrappers.GetOrAdd(type, t =>
            {
                // Get the registered delegate (e.g., Func<Address, Address, bool>)
                var delegateInstance = _comparerManager.Get(t);

                // Infer T from the delegate's parameter type
                var invokeMethod = delegateInstance.GetType().GetMethod("Invoke");
                var parameters = invokeMethod.GetParameters();
                var genericType = parameters[0].ParameterType; // T

                // Build expression: (object a, object b) => delegate((T)a, (T)b)
                var expParamA = Expression.Parameter(typeof(object), "expParamA");
                var expParamB = Expression.Parameter(typeof(object), "expParamB");

                var castA = Expression.Convert(expParamA, genericType);
                var castB = Expression.Convert(expParamB, genericType);

                var call = Expression.Invoke(Expression.Constant(delegateInstance), castA, castB);

                // Compile to Func<object, object, bool>
                var lambda = Expression.Lambda<Func<object, object, bool>>(call, expParamA, expParamB);

                return lambda.Compile();
            });

            return wrapper(original, modified);
        }
    }
}
