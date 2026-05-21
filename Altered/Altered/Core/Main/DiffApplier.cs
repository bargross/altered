using System.Reflection;

namespace Altered.Core.Main
{
    public static class DiffApplier
    {
        // -----------------------------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------------------------

        public static void Apply<T>(T target, List<DiffEntry> diffs)
            where T : class
            => ApplyDiffs(target, diffs);

        // -----------------------------------------------------------------------------------------
        // Internal implementation
        // -----------------------------------------------------------------------------------------

        private static void ApplyDiffs<T>(T target, List<DiffEntry> diffs)
            where T : class
        {
            ValidateArguments(target, diffs);

            var propertiesByName = GetWritableProperties<T>();

            foreach (var diff in diffs)
                TryApplyDiff(propertiesByName, diff, target);
        }

        private static void ValidateArguments<T>(T target, List<DiffEntry> diffs)
            where T : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (diffs == null) throw new ArgumentNullException(nameof(diffs));
            if (diffs.Any(x => x == null)) throw new ArgumentException("Null value in different entries list");
            if (diffs.Any(x => string.IsNullOrWhiteSpace(x.PropertyName))) throw new ArgumentException("Different entries has entry with property name as null, empty or white space in list.");
        }

        private static Dictionary<string, PropertyInfo> GetWritableProperties<T>()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, p => p);
        }

        private static void TryApplyDiff<T>(Dictionary<string, PropertyInfo> propertiesByName, DiffEntry diff, T target)
        {
            if (!propertiesByName.TryGetValue(diff.PropertyName, out var prop))
                return;

            if (!prop.CanWrite)
                return;

            if (!IsTypeCompatible(prop, diff.NewValue))
                return;

            prop.SetValue(target, diff.NewValue);
        }

        private static bool IsTypeCompatible(PropertyInfo prop, object? newValue)
        {
            if (newValue == null)
                return true;

            return prop.PropertyType.IsAssignableFrom(newValue.GetType());
        }
    }
}