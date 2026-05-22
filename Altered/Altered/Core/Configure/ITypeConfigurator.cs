using System.Linq.Expressions;

namespace Altered.Core.Configure
{
    public interface ITypeConfigurator
    {
        Type Type { get; }
        ITypeConfigurator Configure<TValue>();
        ITypeConfigurator Configure(Type type);
        bool IsIgnored<TValue>(string propertyName);
        bool IsIgnored(Type type, string propertyName);
        bool IsIncluded<TValue>(string propertyName);
        bool IsIncluded(Type type, string propertyName);
        ITypeConfigurator Ignore<TValue>(Expression<Func<TValue, object>> propertyIgnoreSelector);
        ITypeConfigurator IgnoreMany<TValue>(params Expression<Func<TValue, object>>[] propertyIgnoreSelectors);
        ITypeConfigurator Include<TValue>(Expression<Func<TValue, object>> propertyIgnoreSelector);
        ITypeConfigurator IncludeMany<TValue>(params Expression<Func<TValue, object>>[] propertyIncludeSelectors);
        void Clear();
    }
}
