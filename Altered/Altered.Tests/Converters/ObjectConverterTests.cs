using Altered.Extensions;
using Altered.Converters;
using Altered.Main;
using System.Text.Json;

namespace Altered.Tests.Converters
{
    public class ObjectConverterTests
    {
        private static JsonSerializerOptions CreateOptionsWithConverter()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            options.Converters.Add(new ObjectConverter());
            options.Converters.Add(new DiffEntryConverter());

            return options;
        }

        // ========== Serialization tests ==========

        [Fact]
        public void Serialize_NullValue_WritesNull()
        {
            var options = CreateOptionsWithConverter();
            object? value = null;

            var json = JsonSerializer.Serialize(value, options);

            Assert.Equal("null", json);
        }

        [Fact]
        public void Serialize_DateTime_WritesIsoString()
        {
            var options = CreateOptionsWithConverter();
            var date = new DateTime(2025, 5, 14, 10, 30, 0, DateTimeKind.Utc);

            var json = JsonSerializer.Serialize(date, options);

            // Accept either with or without milliseconds
            Assert.True(json == "\"2025-05-14T10:30:00.000Z\"" || json == "\"2025-05-14T10:30:00Z\"",
                $"Unexpected JSON: {json}");
        }

        [Fact]
        public void Serialize_DateTimeOffset_WritesIsoString()
        {
            var options = CreateOptionsWithConverter();
            var dateOffset = new DateTimeOffset(2025, 5, 14, 10, 30, 0, TimeSpan.Zero);

            var json = JsonSerializer.Serialize(dateOffset, options);

            Assert.True(
                json == "\"2025-05-14T10:30:00.000Z\"" ||
                json == "\"2025-05-14T10:30:00Z\"" ||
                json == "\"2025-05-14T10:30:00.000+00:00\"" ||
                json == "\"2025-05-14T10:30:00+00:00\"",
                $"Unexpected JSON: {json}");
        }

        [Fact]
        public void Serialize_String_WritesQuotedString()
        {
            var options = CreateOptionsWithConverter();
            var value = "hello";
            var json = JsonSerializer.Serialize(value, options);

            Assert.Equal("\"hello\"", json);
        }

        [Fact]
        public void Serialize_Int_WritesNumber()
        {
            var options = CreateOptionsWithConverter();
            int value = 123;

            var json = JsonSerializer.Serialize(value, options);

            Assert.Equal("123", json);
        }

        [Fact]
        public void Serialize_Bool_WritesTrueFalse()
        {
            var options = CreateOptionsWithConverter();

            var jsonTrue = JsonSerializer.Serialize(true, options);
            var jsonFalse = JsonSerializer.Serialize(false, options);

            Assert.Equal("true", jsonTrue);
            Assert.Equal("false", jsonFalse);
        }

        [Fact]
        public void Serialize_ComplexObject_UsesDefaultSerialization()
        {
            var options = CreateOptionsWithConverter();
            var obj = new { Name = "Test", Value = 42 };

            var json = JsonSerializer.Serialize(obj, options);

            Assert.Contains("\"name\"", json);
            Assert.Contains("\"test\"", json.ToLowerInvariant());
            Assert.Contains("42", json);
        }

        // ========== Deserialization tests ==========

        [Fact]
        public void Deserialize_NullValue_ReturnsNull()
        {
            var options = CreateOptionsWithConverter();
            var json = "null";
            var result = JsonSerializer.Deserialize<object>(json, options);

            Assert.Null(result);
        }

        [Fact]
        public void Deserialize_String_ReturnsString()
        {
            var options = CreateOptionsWithConverter();
            var json = "\"hello\"";

            var result = JsonSerializer.Deserialize<object>(json, options);

            Assert.Equal("hello", result);
        }

        [Fact]
        public void Deserialize_Number_ReturnsNumberType()
        {
            var options = CreateOptionsWithConverter();
            var json = "123";
            
            var result = JsonSerializer.Deserialize<object>(json, options);

            Assert.IsType<int>(result);
            Assert.Equal(123, result);
        }

        [Fact]
        public void Deserialize_DateTime_ReturnsDateTime()
        {
            var options = CreateOptionsWithConverter();
            var json = "\"2025-05-14T10:30:00.000Z\"";

            var result = JsonSerializer.Deserialize<object>(json, options);

            Assert.IsType<DateTime>(result);
            var dt = (DateTime)result!;
            Assert.Equal(2025, dt.Year);
            Assert.Equal(5, dt.Month);
            Assert.Equal(14, dt.Day);
            Assert.Equal(10, dt.Hour);
            Assert.Equal(30, dt.Minute);
            Assert.Equal(DateTimeKind.Utc, dt.Kind);
        }

        // ========== Round-trip tests ==========

        [Fact]
        public void RoundTrip_DateTime_PreservesValue()
        {
            var options = CreateOptionsWithConverter();
            var original = new DateTime(2025, 5, 14, 15, 45, 0, DateTimeKind.Utc);

            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<object>(json, options);

            Assert.IsType<DateTime>(deserialized);

            var result = (DateTime)deserialized!;

            Assert.Equal(original.Ticks, result.Ticks);
            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        [Fact]
        public void RoundTrip_DateTimeOffset_PreservesValue()
        {
            var options = CreateOptionsWithConverter();
            var original = new DateTimeOffset(2025, 5, 14, 15, 45, 0, TimeSpan.FromHours(-5));

            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<object>(json, options);

            Assert.IsType<DateTimeOffset>(deserialized);
            Assert.Equal(original, (DateTimeOffset)deserialized!);
        }

        [Fact]
        public void RoundTrip_ComplexObjectInDiffEntry_PreservesStructure()
        {
            var options = CreateOptionsWithConverter();
            var originalDiff = new DiffEntry
            {
                PropertyName = "Metadata",
                OldValue = new { Version = 1, Active = true },
                NewValue = new { Version = 2, Active = false }
            };

            var json = JsonSerializer.Serialize(originalDiff, options);
            var deserializedDiff = JsonSerializer.Deserialize<DiffEntry>(json, options);

            Assert.NotNull(deserializedDiff);
            Assert.Equal("Metadata", deserializedDiff.PropertyName);

            Assert.IsType<JsonElement>(deserializedDiff.OldValue);
            Assert.IsType<JsonElement>(deserializedDiff.NewValue);

            var oldElement = (JsonElement)deserializedDiff.OldValue;
            var newElement = (JsonElement)deserializedDiff.NewValue;

            Assert.Equal(1, oldElement.GetProperty("version").GetInt32());
            Assert.True(oldElement.GetProperty("active").GetBoolean());
            Assert.Equal(2, newElement.GetProperty("version").GetInt32());
            Assert.False(newElement.GetProperty("active").GetBoolean());
        }

        [Fact]
        public void Converter_RegisteredGlobally_WorksWithDiffJsonExtensions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new ObjectConverter());

            var expectedDate = new DateTime(2025, 5, 14, 10, 30, 0, DateTimeKind.Utc);
            var diffs = new List<DiffEntry>
            {
                new DiffEntry("CreatedAt", null, expectedDate),
                new DiffEntry("Count", 5, 10)
            };

            var json = diffs.ToJson(options);
            var restored = DiffJsonExtensions.FromJson(json, options);

            Assert.Equal(2, restored.Count);
            Assert.Equal("CreatedAt", restored[0].PropertyName);
            Assert.Null(restored[0].OldValue);

            var actualDate = restored[0].NewValue.GetValue<DateTime>();
            Assert.Equal(expectedDate, actualDate);

            Assert.Equal("Count", restored[1].PropertyName);
            Assert.Equal(5, restored[1].OldValue.GetValue<int>());
            Assert.Equal(10, restored[1].NewValue.GetValue<int>());
        }
    }
}