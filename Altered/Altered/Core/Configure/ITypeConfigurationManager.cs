using System.Linq.Expressions;

namespace Altered.Core.Configure
{
    internal interface ITypeConfigurationManager
    {
        ITypeConfigurationManager Configure<TValue>() where TValue : class;
        ITypeConfigurationManager Configure<TValue>(TypeConfigurator configurator) where TValue : class;
        ITypeConfigurationManager IgnoreProperties<TValue>(params Expression<Func<TValue, object>>[] propertySelectors) where TValue : class;
        bool IsTypeConfigured(Type type);
        bool PropertyIsIgnored<TValue>(string propertyName);
        bool PropertyIsIgnored(Type type, string propertyName);
    }
}
