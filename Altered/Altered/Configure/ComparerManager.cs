using System.Collections.Concurrent;

namespace Altered.Configure
{
    internal class ComparerManager
    {
        internal ConcurrentDictionary<Type, Delegate> _customComparers = new();

        public void Register<TValue>(Func<TValue, TValue, bool> customComparer)
        {
            var type = typeof(TValue);
            if (_customComparers.ContainsKey(type))
                throw new ArgumentException($"Type {type.Name} already has a comparer registered.");

            var update = _customComparers.TryAdd(type, customComparer);
            if (!update)
                throw new InvalidOperationException($"Could not configure custom comparer for type {type.Name}");
        }

        public void Replace<TValue>(Func<TValue, TValue, bool> customComparer)
        {
            var type = typeof(TValue);

            if (!_customComparers.ContainsKey(type))
                throw new ArgumentException($"No Comparer for type {type.Name} has been registered.");

            _customComparers[type] = customComparer;
        }

        public bool IsRegistered<TValue>() => IsRegistered(typeof(TValue));
        public bool IsRegistered(Type type) => _customComparers.ContainsKey(type);

        public Delegate Get<TValue>() => Get(typeof(TValue));

        public Delegate Get(Type type)
        {
            if (!_customComparers.ContainsKey(type))
                throw new ArgumentException($"No Comparer for type {type.Name} has been registered.");

            return _customComparers[type];
        }

        public void Clear() => _customComparers.Clear();
    }
}
