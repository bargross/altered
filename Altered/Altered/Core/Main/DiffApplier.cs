using System.Reflection;

namespace Altered.Core.Main
{
    public static class DiffApplier
    {
        public static void Apply<T>(T target, List<DiffEntry> diffs)
            where T : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (diffs == null) throw new ArgumentNullException(nameof(diffs));

            if (diffs.Any(x => x == null)) throw new ArgumentException("Null value in different entries list");

            if (diffs.Any(x => string.IsNullOrWhiteSpace(x.PropertyName))) throw new ArgumentException("Different entries has entry with property name as null, empty or white space in list.");

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propertyDict = properties.ToDictionary(p => p.Name, p => p);

            foreach (var diff in diffs)
            {
                if (!propertyDict.TryGetValue(diff.PropertyName, out var prop))
                    continue;

                if (!prop.CanWrite)
                    continue;

                // Type checking - don't apply if types don't match
                if (diff.NewValue != null && !prop.PropertyType.IsAssignableFrom(diff.NewValue.GetType()))
                    continue;

                prop.SetValue(target, diff.NewValue);
            }
        }
    }
}
