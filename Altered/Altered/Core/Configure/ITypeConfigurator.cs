using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Altered.Core.Configure
{
    public interface ITypeConfigurator
    {
        ITypeConfigurator Ignore<TValue>(Expression<Func<TValue, object>> propertySelector);
        bool IsIgnored<TValue>(string propertyName);
    }
}
