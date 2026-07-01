using System.Globalization;
using System.Reflection;

namespace Altered.Main
{
    public static class DiffApplier
    {
        // -----------------------------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------------------------

        ///
        public static void Apply<T>(T target, List<DiffEntry> diffs)
            => ApplyDiffs(target, diffs);

        // -----------------------------------------------------------------------------------------
        // Internal implementation
        // -----------------------------------------------------------------------------------------

        internal static void ApplyDiffs<T>(T target, List<DiffEntry> diffs)
        {
            ValidateArguments(target, diffs);

            var propertiesByName = GetWritableProperties<T>();

            foreach (var diff in diffs)
                TryApplyDiff(propertiesByName, diff, target);
        }

        internal static void ValidateArguments<T>(T target, List<DiffEntry> diffs)
        {
            if (target == null) 
                throw new ArgumentNullException(nameof(target));

            if (diffs == null) 
                throw new ArgumentNullException(nameof(diffs));

            if (diffs.Any(x => x == null)) 
                throw new ArgumentException("Null value in different entries list");

            if (diffs.Any(x => string.IsNullOrWhiteSpace(x.PropertyName))) 
                throw new ArgumentException("Different entries has entry with property name as null, empty or white space in list.");
        }

        internal static Dictionary<string, PropertyInfo> GetWritableProperties<T>()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, p => p);
        }

        internal static void TryApplyDiff<TValue>(Dictionary<string, PropertyInfo> propertiesByName, DiffEntry diff, TValue target)
        {
            if (!propertiesByName.TryGetValue(diff.PropertyName, out var prop)) return;
            if (!prop.CanWrite) return;

            object? newValue = diff.NewValue;

            // Coerce the value using the type hint if available
            if (!string.IsNullOrEmpty(diff.NewValueTypeHint) && newValue != null)
            {
                if (prop.PropertyType.IsEnum)
                {
                    newValue = Enum.ToObject(prop.PropertyType, newValue);
                }
                else
                {
                    try
                    {
                        newValue = Convert.ChangeType(newValue, prop.PropertyType, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        // Fall through to the compatibility check below
                    }
                }
            }

            if (!IsTypeCompatible(prop, newValue)) 
                return;

            prop.SetValue(target, newValue);
        }

        internal static bool IsTypeCompatible(PropertyInfo prop, object? newValue)
        {
            if (newValue == null)
            {
                // Allow null only if the property is a reference type or a nullable value type
                var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);

                return !prop.PropertyType.IsValueType || underlyingType != null;
            }

            return prop.PropertyType.IsAssignableFrom(newValue.GetType());
        }
    }
}