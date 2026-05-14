using System.Linq.Expressions;

namespace Altered.Core.Configure
{
    public interface ITypeConfigurator
    {
        ITypeConfigurator Ignore<TValue>(Expression<Func<TValue, object>> propertySelector);
        
        bool IsIgnored<TValue>(string propertyName);
        void Clear();

        IReadOnlySet<string> GetIgnoredProperties();
        IReadOnlySet<string> GetIncludedProperties();
    }
}
