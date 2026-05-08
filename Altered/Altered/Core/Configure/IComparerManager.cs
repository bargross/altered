namespace Altered.Core.Configure
{
    internal interface IComparerManager
    {
        void Register<TValue>(Func<TValue, TValue, bool> customComparer);
        Delegate Get<TValue>();
    }
}
