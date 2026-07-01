using Altered.Converters;
using Altered.Main;
using System.Text.Json;
using Xunit;

namespace Altered.Tests
{
    public class DiffEntryConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public DiffEntryConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new DiffEntryConverter() }
            };
        }

        // Helper to round-trip a DiffEntry
        private DiffEntry RoundTrip(DiffEntry entry)
        {
            string json = JsonSerializer.Serialize(entry, _options);
            return JsonSerializer.Deserialize<DiffEntry>(json, _options);
        }

        // -----------------------------------------------------------------
        // Helpers to extract values from JsonElement
        // -----------------------------------------------------------------

        private T GetValue<T>(object value) => ((JsonElement)value).Deserialize<T>(_options);
        private int GetInt(object value) => ((JsonElement)value).GetInt32();
        private long GetLong(object value) => ((JsonElement)value).GetInt64();
        private float GetFloat(object value) => ((JsonElement)value).GetSingle();
        private double GetDouble(object value) => ((JsonElement)value).GetDouble();
        private decimal GetDecimal(object value) => ((JsonElement)value).GetDecimal();
        private T GetEnum<T>(object value) where T : struct, Enum => (T)(object)((JsonElement)value).GetInt32();
        private DateTime GetDateTime(object value) => ((JsonElement)value).GetDateTime();

        // -----------------------------------------------------------------
        // Type hint writing for problematic types
        // -----------------------------------------------------------------

        [Fact]
        public void Write_IntNewValue_WritesTypeHint()
        {
            var entry = DiffEntryBuilder.Create("Prop", oldValue: null, newValue: 42);
            var result = RoundTrip(entry);

            Assert.Equal(42, GetInt(result.NewValue));
            Assert.Equal(typeof(int).AssemblyQualifiedName, result.NewValueTypeHint);
        }

        [Fact]
        public void Write_LongNewValue_WritesTypeHint()
        {
            var entry = DiffEntryBuilder.Create("Prop", oldValue: null, newValue: 42L);
            var result = RoundTrip(entry);

            Assert.Equal(42L, GetLong(result.NewValue));
            Assert.Equal(typeof(long).AssemblyQualifiedName, result.NewValueTypeHint);
        }

        [Fact]
        public void Write_FloatNewValue_WritesTypeHint()
        {
            var entry = DiffEntryBuilder.Create("Prop", oldValue: null, newValue: 3.14f);
            var result = RoundTrip(entry);

            Assert.Equal(3.14f, GetFloat(result.NewValue));
            Assert.Equal(typeof(float).AssemblyQualifiedName, result.NewValueTypeHint);
        }

        [Fact]
        public void Write_DoubleNewValue_WritesTypeHint()
        {
            var entry = DiffEntryBuilder.Create("Prop", oldValue: null, newValue: 3.14);
            var result = RoundTrip(entry);

            Assert.Equal(3.14, GetDouble(result.NewValue));
            Assert.Equal(typeof(double).AssemblyQualifiedName, result.NewValueTypeHint);
        }

        [Fact]
        public void Write_DecimalNewValue_WritesTypeHint()
        {
            var entry = DiffEntryBuilder.Create("Prop", oldValue: null, newValue: 3.14m);
            var result = RoundTrip(entry);

            Assert.Equal(3.14m, GetDecimal(result.NewValue));
            Assert.Equal(typeof(decimal).AssemblyQualifiedName, result.NewValueTypeHint);
        }

        [Fact]
        public void Write_EnumNewValue_WritesTypeHint()
        {
            var entry = DiffEntryBuilder.Create("Prop", oldValue: null, newValue: TestEnum.Value2);
            var result = RoundTrip(entry);

            Assert.Equal(TestEnum.Value2, GetEnum<TestEnum>(result.NewValue));
            Assert.Equal(typeof(TestEnum).AssemblyQualifiedName, result.NewValueTypeHint);
        }

        // -----------------------------------------------------------------
        // Type hint NOT written for non‑problematic types
        // -----------------------------------------------------------------

        [Fact]
        public void Write_StringNewValue_NoTypeHint()
        {
            var entry = DiffEntryBuilder.Create("Prop", oldValue: null, newValue: "hello");
            var result = RoundTrip(entry);

            Assert.Equal("hello", ((JsonElement)result.NewValue).GetString());
            Assert.Null(result.NewValueTypeHint);
        }

        [Fact]
        public void Write_DateTimeNewValue_NoTypeHint()
        {
            var now = DateTime.UtcNow;
            var entry = DiffEntryBuilder.Create("Prop", oldValue: null, newValue: now);
            var result = RoundTrip(entry);

            Assert.Equal(now, GetDateTime(result.NewValue));
            Assert.Null(result.NewValueTypeHint);
        }

        [Fact]
        public void Write_CustomObjectNewValue_NoTypeHint()
        {
            var obj = new CustomObject { Name = "Test", Value = 99 };
            var entry = DiffEntryBuilder.Create("Prop", oldValue: null, newValue: obj);
            var result = RoundTrip(entry);

            var jsonElement = Assert.IsType<JsonElement>(result.NewValue);
            var deserializedObj = jsonElement.Deserialize<CustomObject>(_options);
            Assert.Equal(obj.Name, deserializedObj.Name);
            Assert.Equal(obj.Value, deserializedObj.Value);
            Assert.Null(result.NewValueTypeHint);
        }

        // -----------------------------------------------------------------
        // OldValue (should not affect hint writing; hint only for NewValue)
        // -----------------------------------------------------------------

        [Fact]
        public void Write_OldValueProblematic_NoHintForOldValue()
        {
            var entry = DiffEntryBuilder.Create("Prop", oldValue: 100, newValue: "hello");
            var result = RoundTrip(entry);

            Assert.Equal(100, GetInt(result.OldValue));
            Assert.Equal("hello", ((JsonElement)result.NewValue).GetString());
            Assert.Null(result.NewValueTypeHint);
        }

        // -----------------------------------------------------------------
        // Null values
        // -----------------------------------------------------------------

        [Fact]
        public void Write_NullNewValue_NoTypeHint()
        {
            var entry = DiffEntryBuilder.Create("Prop", oldValue: "old", newValue: null);
            var result = RoundTrip(entry);

            Assert.Null(result.NewValue);
            Assert.Null(result.NewValueTypeHint);
        }

        [Fact]
        public void Write_NullOldValue_HandledCorrectly()
        {
            var entry = DiffEntryBuilder.Create("Prop", oldValue: null, newValue: 123);
            var result = RoundTrip(entry);

            Assert.Null(result.OldValue);
            Assert.Equal(123, GetInt(result.NewValue));
            Assert.Equal(typeof(int).AssemblyQualifiedName, result.NewValueTypeHint);
        }

        // -----------------------------------------------------------------
        // PropertyName handling
        // -----------------------------------------------------------------

        [Fact]
        public void Write_EmptyPropertyName_IsPreserved()
        {
            var entry = DiffEntryBuilder.Create("", newValue: 5);
            var result = RoundTrip(entry);

            Assert.Equal("", result.PropertyName);
            Assert.Equal(5, GetInt(result.NewValue));
            Assert.Equal(typeof(int).AssemblyQualifiedName, result.NewValueTypeHint);
        }

        [Fact]
        public void Write_NullPropertyName_BecomesEmptyStringOnDeserialize()
        {
            var entry = new DiffEntry { PropertyName = null, NewValue = 5 };
            var result = RoundTrip(entry);

            Assert.Equal(string.Empty, result.PropertyName);
            Assert.Equal(5, GetInt(result.NewValue));
        }

        // -----------------------------------------------------------------
        // Roundtrip with both Old and New values of problematic types
        // -----------------------------------------------------------------

        [Fact]
        public void Write_BothValuesProblematic_HintOnlyForNewValue()
        {
            var entry = DiffEntryBuilder.Create("Prop", oldValue: 1.5f, newValue: 2.5f);
            var result = RoundTrip(entry);

            Assert.Equal(1.5f, GetFloat(result.OldValue));
            Assert.Equal(2.5f, GetFloat(result.NewValue));
            Assert.Equal(typeof(float).AssemblyQualifiedName, result.NewValueTypeHint);
        }

        // -----------------------------------------------------------------
        // Integration with ObjectConverter
        // -----------------------------------------------------------------

        [Fact]
        public void Write_WithObjectConverter_DateTimeHandlingWorks()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new ObjectConverter());
            options.Converters.Add(new DiffEntryConverter());

            var date = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var entry = new DiffEntry { PropertyName = "Date", NewValue = date };
            var json = JsonSerializer.Serialize(entry, options);
            var result = JsonSerializer.Deserialize<DiffEntry>(json, options);

            // Determine the actual type of result.NewValue
            DateTime deserializedDate;
            if (result.NewValue is DateTime dt)
            {
                deserializedDate = dt;
            }
            else
            {
                // Assume it's a JsonElement
                var element = Assert.IsType<JsonElement>(result.NewValue);
                deserializedDate = element.GetDateTime();
            }

            Assert.Equal(date, deserializedDate);
            Assert.Equal(DateTimeKind.Utc, deserializedDate.Kind);
            Assert.Null(result.NewValueTypeHint);
        }

        // -----------------------------------------------------------------
        // Edge case: NewValue is an array
        // -----------------------------------------------------------------

        [Fact]
        public void Write_ArrayNewValue_NoTypeHint()
        {
            var array = new int[] { 1, 2, 3 };
            var entry = DiffEntryBuilder.Create("Prop", newValue: array);
            var result = RoundTrip(entry);

            var jsonElement = Assert.IsType<JsonElement>(result.NewValue);
            var deserializedArray = jsonElement.EnumerateArray().Select(e => e.GetInt32()).ToArray();

            Assert.Equal(array, deserializedArray);
            Assert.Null(result.NewValueTypeHint);
        }
    }
}