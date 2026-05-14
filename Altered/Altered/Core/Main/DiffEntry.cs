using System.Text.Json.Serialization;

namespace Altered.Core.Main
{
    public class DiffEntry
    {
        public string PropertyName { get; set; }
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }

        [JsonConstructor]
        public DiffEntry() { }

        public DiffEntry(string propertyName, object? oldValue, object? newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        // For debugging and serialization
        public override string ToString()
            => $"{PropertyName}: {OldValue} -> {NewValue}";
    }
}
