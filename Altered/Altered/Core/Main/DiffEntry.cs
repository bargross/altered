using System.Text.Json.Serialization;

namespace Altered.Core.Main
{
    public class DiffEntry
    {
        /// <summary>
        /// name of the property that has changed. This is a required field and should not be null, empty, or whitespace. It serves as an identifier for the property being compared and is crucial for applying the diff correctly.
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// contains the original value of the property before the change. This field can be null, which may indicate that the property was not set or was set to null in the original object. It is important for understanding what the previous state of the property was before the diff is applied.
        /// </summary>
        public object? OldValue { get; set; }

        /// <summary>
        /// contains the new value of the property after the change. This field can be null, which may indicate that the property has been removed or set to null in the modified object. It is essential for understanding what the new state of the property is after the diff is applied.
        /// </summary>
        public object? NewValue { get; set; }

        [JsonConstructor]
        public DiffEntry() { }

        public DiffEntry(string propertyName, object? oldValue, object? newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// ToString is overridden to provide a more informative string representation of the DiffEntry, which can be useful for debugging and logging purposes. It shows the property name along with its old and new values in a clear format.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ToDebugString();

        internal string ToDebugString()
            => $"{PropertyName}: {OldValue} -> {NewValue}";
    }
}
