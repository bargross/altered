using Altered.Core.Attributes;


namespace Altered.Core.Main
{
    using System.Reflection;

    public static class DiffGenerator
    {
        public static List<DiffEntry> Generate<T>(T original, T modified)
            where T : class
        {
            if (original == null && modified == null) return new List<DiffEntry>();
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (modified == null) throw new ArgumentNullException(nameof(modified));

            var diffs = new List<DiffEntry>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // Skip properties with [IgnoreInDiff] attribute
                if (prop.GetCustomAttribute<IgnoreInDiffAttribute>() != null)
                    continue;

                // Skip properties without a public getter
                if (!prop.CanRead) continue;

                var oldValue = prop.GetValue(original);
                var newValue = prop.GetValue(modified);

                // Simple value comparison
                if (!AreEqual(oldValue, newValue))
                {
                    diffs.Add(new DiffEntry
                    {
                        PropertyName = prop.Name,
                        OldValue = oldValue,
                        NewValue = newValue
                    });
                }
            }

            return diffs;
        }

        private static bool AreEqual(object a, object b)
        {
            // Both null
            if (a == null && b == null) return true;

            // One null, one not
            if (a == null || b == null) return false;

            // Value types and strings
            return a.Equals(b);
        }
    }
}
