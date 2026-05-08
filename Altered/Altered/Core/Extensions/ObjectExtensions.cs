using Altered.Core.Main;
using System.Linq.Expressions;

namespace Altered.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static List<DiffEntry> Generate<TValue>(this TValue original, TValue modified, params Expression<Func<TValue, object>>[] propertySelectors) where TValue : class
        {
            return DiffGenerator.Generate(original, modified, propertySelectors);
        }
    }
}
