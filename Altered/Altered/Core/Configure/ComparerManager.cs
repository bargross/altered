namespace Altered.Core.Configure
{
    internal class ComparerManager: IComparerManager
    {
        public Dictionary<Type, Delegate> _customComparers;

        public ComparerManager(Dictionary<Type, Delegate> customComparers) 
        {
            _customComparers = customComparers;
        }

        public ComparerManager()
        {
            _customComparers = new Dictionary<Type, Delegate>();
        }

        public void Register<TValue>(Func<TValue, TValue, bool> customComparer)
        {
            var type = typeof(TValue);
            if (_customComparers.ContainsKey(type))
                throw new ArgumentException($"Type {type.Name} already has a comparer registered.");

            _customComparers.Add(type, customComparer);
        }

        public void Replace<TValue>(Func<TValue, TValue, bool> customComparer)
        {
            var type = typeof(TValue);

            if (_customComparers.ContainsKey(type))
                throw new ArgumentException("No Comparer for type {type.Name} has been registered.");

            _customComparers[type] = customComparer;
        }

        public bool IsRegistered<TValue>() => IsRegistered(typeof(TValue));
        public bool IsRegistered(Type type) => _customComparers.ContainsKey(type);

        public Delegate Get<TValue>() => Get(typeof(TValue));

        public Delegate Get(Type type)
        {
            if (_customComparers.ContainsKey(type))
                throw new ArgumentException("No Comparer for type {type.Name} has been registered.");

            return _customComparers[type];
        }
    }
}
