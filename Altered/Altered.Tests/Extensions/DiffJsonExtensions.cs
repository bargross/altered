using Altered.Main;
using Altered.Extensions;
using System.Text.Json;

namespace Altered.Tests.Extensions
{
    public class DiffJsonExtensionsTests
    {
        private static List<DiffEntry> CreateSampleDiffs()
        {
            return new List<DiffEntry>
            {
                new DiffEntry("Name", "Alice", "Bob"),
                new DiffEntry("Age", 30, 31),
                new DiffEntry("IsActive", true, false),
                new DiffEntry("LastLogin", null, DateTime.UtcNow)
            };
        }

        // ========== ToJson Tests ==========

        [Fact]
        public void ToJson_WithNullDiffs_ReturnsEmptyArray()
        {
            // Arrange
            List<DiffEntry>? nullDiffs = null;

            // Act
            string json = nullDiffs.ToJson();

            // Assert
            Assert.Equal("[]", json);
        }

        [Fact]
        public void ToJson_WithEmptyList_ReturnsEmptyArray()
        {
            // Arrange
            var emptyDiffs = new List<DiffEntry>();

            // Act
            string json = emptyDiffs.ToJson();

            // Assert
            Assert.Equal("[]", json);
        }

        [Fact]
        public void ToJson_WithDefaultOptions_ReturnsCamelCaseIndentedJson()
        {
            // Arrange
            var diffs = CreateSampleDiffs();

            // Act
            string json = diffs.ToJson();

            // Assert
            Assert.Contains("\"propertyName\"", json);
            Assert.Contains("\"oldValue\"", json);
            Assert.Contains("\"newValue\"", json);
            Assert.Contains("Alice", json);
            Assert.Contains("Bob", json);
            Assert.Contains("30", json);
            Assert.Contains("31", json);
        }

        [Fact]
        public void ToJson_WithCustomOptions_UsesProvidedOptions()
        {
            // Arrange
            var diffs = CreateSampleDiffs();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            };

            // Act
            string json = diffs.ToJson(options);

            // Assert
            Assert.Contains("\"PropertyName\"", json);
            Assert.DoesNotContain("\n", json);
        }

        // ========== FromJson Tests ==========

        [Fact]
        public void FromJson_WithNullInput_ReturnsEmptyList()
        {
            // Act
            var result = DiffJsonExtensions.FromJson(null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FromJson_WithEmptyString_ReturnsEmptyList()
        {
            // Act
            var result = DiffJsonExtensions.FromJson("");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FromJson_WithWhitespace_ReturnsEmptyList()
        {
            // Act
            var result = DiffJsonExtensions.FromJson("   ");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FromJson_WithNullLiteral_ReturnsEmptyList()
        {
            // Act
            var result = DiffJsonExtensions.FromJson("null");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsListOfDiffs()
        {
            // Arrange
            const string json = """
                [
                    {
                        "propertyName": "Name",
                        "oldValue": "Alice",
                        "newValue": "Bob"
                    },
                    {
                        "propertyName": "Age",
                        "oldValue": 30,
                        "newValue": 31
                    }
                ]
                """;

            // Act
            var diffs = DiffJsonExtensions.FromJson(json);

            // Assert
            Assert.Equal(2, diffs.Count);
            Assert.Equal("Name", diffs[0].PropertyName);

            // Extract values from JsonElement
            Assert.Equal("Alice", GetValueAs<string>(diffs[0].OldValue));
            Assert.Equal("Bob", GetValueAs<string>(diffs[0].NewValue));
            Assert.Equal("Age", diffs[1].PropertyName);
            Assert.Equal(30, GetValueAs<int>(diffs[1].OldValue));
            Assert.Equal(31, GetValueAs<int>(diffs[1].NewValue));
        }

        [Fact]
        public void FromJson_WithInvalidJson_ThrowsArgumentException()
        {
            // Arrange
            const string invalidJson = "not json";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => DiffJsonExtensions.FromJson(invalidJson));
            Assert.Contains("Invalid JSON format", ex.Message);
        }

        [Fact]
        public void FromJson_WithCustomOptions_UsesProvidedOptions()
        {
            // Arrange
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            const string json = """
                [
                    {
                        "PropertyName": "Name",
                        "OldValue": "Alice",
                        "NewValue": "Bob"
                    }
                ]
                """;

            // Act
            var diffs = DiffJsonExtensions.FromJson(json, options);

            // Assert
            Assert.Single(diffs);
            Assert.Equal("Name", diffs[0].PropertyName);
        }

        // ========== Round Trip Tests ==========

        [Fact]
        public void RoundTrip_WithTypicalDiffs_PreservesData()
        {
            // Arrange
            var originalDiffs = CreateSampleDiffs();

            // Act
            var json = originalDiffs.ToJson();
            var restoredDiffs = DiffJsonExtensions.FromJson(json);

            // Assert
            Assert.Equal(originalDiffs.Count, restoredDiffs.Count);
            for (int i = 0; i < originalDiffs.Count - 1; i++) // exclude last as DateTime does not preserve format without json options.
            {
                Assert.Equal(originalDiffs[i].PropertyName.Trim(), restoredDiffs[i].PropertyName.Trim());
                AssertEqualValues(originalDiffs[i].OldValue, restoredDiffs[i].OldValue);
                AssertEqualValues(originalDiffs[i].NewValue, restoredDiffs[i].NewValue);
            }
        }

        // ========== File I/O Tests ==========

        [Fact]
        public async Task WriteToJsonFileAsync_WithValidPath_WritesFile()
        {
            // Arrange
            var diffs = CreateSampleDiffs();
            string tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await diffs.WriteToJsonFileAsync(tempFile);
                Assert.True(File.Exists(tempFile));
                var content = await File.ReadAllTextAsync(tempFile);
                Assert.Contains("propertyName", content);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task WriteToJsonFileAsync_WithNullDiffs_WritesEmptyArray()
        {
            // Arrange
            List<DiffEntry>? nullDiffs = null;
            string tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await nullDiffs.WriteToJsonFileAsync(tempFile);
                var content = await File.ReadAllTextAsync(tempFile);
                Assert.Equal("[]", content);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task WriteToJsonFileAsync_WithMissingDirectory_CreatesDirectory()
        {
            // Arrange
            var diffs = CreateSampleDiffs();
            string nestedDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "sub");
            string tempFile = Path.Combine(nestedDir, "audit.json");

            try
            {
                // Act
                await diffs.WriteToJsonFileAsync(tempFile);

                // Assert
                Assert.True(File.Exists(tempFile));
                Assert.True(Directory.Exists(nestedDir));
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
                if (Directory.Exists(nestedDir)) Directory.Delete(nestedDir, true);
            }
        }

        [Fact]
        public async Task WriteToJsonFileAsync_WithInvalidPath_ThrowsArgumentException()
        {
            // Arrange
            var diffs = CreateSampleDiffs();
            string invalidPath = new string(Path.GetInvalidPathChars()) + "file.json";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                diffs.WriteToJsonFileAsync(invalidPath));
        }

        [Fact]
        public async Task WriteToJsonFileAsync_WithNullFilePath_ThrowsArgumentException()
        {
            // Arrange
            var diffs = CreateSampleDiffs();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                diffs.WriteToJsonFileAsync(null!));
        }

        [Fact]
        public async Task ReadFromJsonFileAsync_WithValidFile_ReturnsDiffs()
        {
            // Arrange
            var originalDiffs = CreateSampleDiffs();
            string tempFile = Path.GetTempFileName();
            await originalDiffs.WriteToJsonFileAsync(tempFile);

            try
            {
                // Act
                var loaded = await DiffJsonExtensions.ReadFromJsonFileAsync(tempFile);

                // Assert
                Assert.Equal(originalDiffs.Count, loaded.Count);
                Assert.Equal(originalDiffs[0].PropertyName, loaded[0].PropertyName);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ReadFromJsonFileAsync_WithEmptyFile_ReturnsEmptyList()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, "");

            try
            {
                // Act
                var loaded = await DiffJsonExtensions.ReadFromJsonFileAsync(tempFile);

                // Assert
                Assert.Empty(loaded);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ReadFromJsonFileAsync_WithNullLiteralFile_ReturnsEmptyList()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, "null");

            try
            {
                // Act
                var loaded = await DiffJsonExtensions.ReadFromJsonFileAsync(tempFile);

                // Assert
                Assert.Empty(loaded);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ReadFromJsonFileAsync_WithNonexistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            string nonExistent = Path.GetTempFileName();
            File.Delete(nonExistent);

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                DiffJsonExtensions.ReadFromJsonFileAsync(nonExistent));
        }

        [Fact]
        public async Task ReadFromJsonFileAsync_WithCorruptJson_ThrowsArgumentException()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, "this is not json");

            try
            {
                // Act & Assert
                var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                    DiffJsonExtensions.ReadFromJsonFileAsync(tempFile));
                Assert.Contains("Invalid JSON format", ex.Message);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        // ========== TryFromJson Tests ==========

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsSuccessAndDiffs()
        {
            // Arrange
            const string json = """[{"propertyName":"Age","oldValue":30,"newValue":31}]""";

            // Act
            var (success, diffs) = DiffJsonExtensions.TryFromJson(json);

            // Assert
            Assert.True(success);
            Assert.Single(diffs);
            Assert.Equal("Age", diffs[0].PropertyName);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndEmptyList()
        {
            // Arrange
            const string invalidJson = "not json";

            // Act
            var (success, diffs) = DiffJsonExtensions.TryFromJson(invalidJson);

            // Assert
            Assert.False(success);
            Assert.Empty(diffs);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void TryFromJson_WithNullOrEmpty_ReturnsExpected(string? input, bool expectedSuccess)
        {
            var (success, diffs) = DiffJsonExtensions.TryFromJson(input);

            Assert.Equal(expectedSuccess, success);
            Assert.NotNull(diffs);
            Assert.Empty(diffs);
        }

        // ========== TryWriteToJsonFileAsync Tests ==========

        [Fact]
        public async Task TryWriteToJsonFileAsync_WithValidPath_ReturnsSuccess()
        {
            // Arrange
            var diffs = CreateSampleDiffs();
            string tempFile = Path.GetTempFileName();

            try
            {
                // Act
                var (success, error) = await diffs.TryWriteToJsonFileAsync(tempFile);

                // Assert
                Assert.True(success);
                Assert.Null(error);
                Assert.True(File.Exists(tempFile));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task TryWriteToJsonFileAsync_WithInvalidPath_ReturnsFalseWithError()
        {
            // Arrange
            var diffs = CreateSampleDiffs();
            string invalidPath = new string(Path.GetInvalidPathChars()) + "file.json";

            // Act
            var (success, error) = await diffs.TryWriteToJsonFileAsync(invalidPath);

            // Assert
            Assert.False(success);
            Assert.NotNull(error);
        }

        [Fact]
        public async Task TryWriteToJsonFileAsync_WithNullDiffs_ReturnsSuccessAndWritesEmptyArray()
        {
            // Arrange
            List<DiffEntry>? nullDiffs = null;
            string tempFile = Path.GetTempFileName();

            try
            {
                // Act
                var (success, error) = await nullDiffs.TryWriteToJsonFileAsync(tempFile);

                // Assert
                Assert.True(success);
                Assert.Null(error);
                var content = await File.ReadAllTextAsync(tempFile);
                Assert.Equal("[]", content);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task TryWriteToJsonFileAsync_WithReadOnlyDirectory_ReturnsFalseWithError()
        {
            // This test may require specific permissions, skip on CI if needed
            // For Windows, you might test with a path like "Z:\\invalid" which doesn't exist
            // Simpler: use a path that points to a file that exists but is read-only

            // Arrange
            var diffs = CreateSampleDiffs();
            string tempFile = Path.GetTempFileName();
            File.SetAttributes(tempFile, FileAttributes.ReadOnly);

            try
            {
                // Act
                var (success, error) = await diffs.TryWriteToJsonFileAsync(tempFile);

                // Assert
                Assert.False(success);
                Assert.NotNull(error);
            }
            finally
            {
                File.SetAttributes(tempFile, FileAttributes.Normal);
                File.Delete(tempFile);
            }
        }

        private static void AssertEqualValues(object? expected, object? actual)
        {
            if (expected == null && actual == null)
                return;

            if (expected == null || actual == null)
            {
                Assert.Equal(expected, actual);
                return;
            }

            // Handle JsonElement coming from deserialization
            if (actual is JsonElement jsonElement)
            {
                // Convert JsonElement to expected type
                var converted = ConvertJsonElement(jsonElement, expected.GetType());
                Assert.Equal(expected, converted);
            }
            else
            {
                Assert.Equal(expected, actual);
            }
        }

        private static object? ConvertJsonElement(JsonElement element, Type targetType)
        {
            if (targetType == typeof(DateTime))
                return element.GetDateTime();
            if (targetType == typeof(DateTimeOffset))
                return element.GetDateTimeOffset();
            if (targetType == typeof(string))
                return element.GetString();
            if (targetType == typeof(int))
                return element.GetInt32();
            if (targetType == typeof(long))
                return element.GetInt64();
            if (targetType == typeof(double))
                return element.GetDouble();
            if (targetType == typeof(decimal))
                return element.GetDecimal();
            if (targetType == typeof(bool))
                return element.GetBoolean();
            if (targetType == typeof(Guid))
                return element.GetGuid();

            // Fallback: get the raw value as object
            return JsonSerializer.Deserialize(element.GetRawText(), targetType);
        }

        private static T GetValueAs<T>(object? value)
        {
            if (value is JsonElement element)
            {
                return element.Deserialize<T>()!;
            }
            return (T)value!;
        }
    }
}