using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Altered.Core.Extensions
{
    public static class ExpressionExtensions
    {
        public static string GetPropertyName<TValue>(this Expression<Func<TValue, object>> propertySelector)
        {
            switch (propertySelector.Body)
            {
                case MemberExpression memberExpression:
                    return memberExpression.Member.Name;

                case UnaryExpression unaryExpression when unaryExpression.Operand is MemberExpression memberExpression:
                    return memberExpression.Member.Name;

                default:
                    throw new ArgumentException($"Expression '{propertySelector}' does not refer to a property");
            }
        }        
    }
}
