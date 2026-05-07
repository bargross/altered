namespace Altered.Core.Main
{
    public class DiffEntry
    {
        public string? PropertyName { get; set; }
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }

        // For debugging and serialization
        public override string ToString()
            => $"{PropertyName}: {OldValue} -> {NewValue}";
    }
}
