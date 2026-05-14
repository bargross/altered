using Altered.Core.Main;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Altered.Core.Extensions
{
    public static class DiffJsonExtensions
    {
        private static readonly JsonSerializerOptions DefaultJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes a list of diffs to a JSON string.
        /// </summary>
        /// <param name="diffs">The diffs to serialize. Can be null (returns empty array "[]").</param>
        /// <param name="options">Optional JSON serializer options. Uses defaults if null.</param>
        /// <returns>JSON string representation of the diffs. Never returns null.</returns>
        public static string ToJson(this List<DiffEntry>? diffs, JsonSerializerOptions? options = null)
        {
            // Safety: null diffs become empty array
            if (diffs == null || diffs.Count == 0)
            {
                return "[]";
            }

            try
            {
                return JsonSerializer.Serialize(diffs, options ?? DefaultJsonOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to serialize diffs to JSON.", ex);
            }
        }

        /// <summary>
        /// Deserializes a JSON string back to a list of diffs.
        /// </summary>
        /// <param name="json">JSON string. Can be null or whitespace (returns empty list).</param>
        /// <param name="options">Optional JSON serializer options. Uses defaults if null.</param>
        /// <returns>List of DiffEntry, never returns null.</returns>
        public static List<DiffEntry> FromJson(string? json, JsonSerializerOptions? options = null)
        {
            // Safety: null, empty, or whitespace input returns empty list
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<DiffEntry>();
            }

            // Safety: "null" literal also returns empty list
            if (json.Trim() == "null")
            {
                return new List<DiffEntry>();
            }

            try
            {
                var result = JsonSerializer.Deserialize<List<DiffEntry>>(json, options ?? DefaultJsonOptions);
                return result ?? new List<DiffEntry>();
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format. Unable to deserialize diffs. JSON: {json.Truncate(100)}", ex);
            }
        }

        /// <summary>
        /// Writes diffs to a file as JSON.
        /// </summary>
        /// <param name="diffs">The diffs to write. Can be null (writes empty array).</param>
        /// <param name="filePath">Path to the output file. Cannot be null or empty.</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="ArgumentNullException">Thrown if filePath is null.</exception>
        /// <exception cref="ArgumentException">Thrown if filePath is empty or contains invalid characters.</exception>
        public static async Task WriteToJsonFileAsync(
            this List<DiffEntry>? diffs,
            string filePath,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            // Safety: validate file path
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            // Safety: check for invalid path characters
            var invalidChars = Path.GetInvalidPathChars();
            if (filePath.IndexOfAny(invalidChars) >= 0)
                throw new ArgumentException("File path contains invalid characters.", nameof(filePath));

            // Safety: ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = diffs.ToJson(options);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }

        /// <summary>
        /// Reads diffs from a JSON file.
        /// </summary>
        /// <param name="filePath">Path to the input file. Cannot be null or empty.</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of DiffEntry. Returns empty list if file is empty or contains only whitespace.</returns>
        /// <exception cref="FileNotFoundException">Thrown if file does not exist.</exception>
        /// <exception cref="ArgumentNullException">Thrown if filePath is null.</exception>
        /// <exception cref="ArgumentException">Thrown if filePath is empty or contains invalid characters.</exception>
        public static async Task<List<DiffEntry>> ReadFromJsonFileAsync(
            string filePath,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            // Safety: validate file path
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            // Safety: check for file existence
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}", filePath);

            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return FromJson(json, options);
        }

        /// <summary>
        /// Tries to deserialize JSON safely without throwing exceptions.
        /// </summary>
        /// <param name="json">JSON string to deserialize.</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <returns>A tuple: (Success, Diffs). Diffs is empty list if failed.</returns>
        public static (bool Success, List<DiffEntry> Diffs) TryFromJson(
            string? json,
            JsonSerializerOptions? options = null)
        {
            try
            {
                var diffs = FromJson(json, options);

                if (diffs.Count == 0)
                {
                    return (false, diffs);
                }

                return (true, diffs);
            }
            catch (ArgumentException)
            {
                return (false, new List<DiffEntry>());
            }
            catch (JsonException)
            {
                return (false, new List<DiffEntry>());
            }
        }

        /// <summary>
        /// Tries to write diffs to a file safely without throwing exceptions.
        /// </summary>
        /// <param name="diffs">The diffs to write.</param>
        /// <param name="filePath">Path to the output file.</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A tuple: (Success, ErrorMessage). Success is true if write succeeded.</returns>
        public static async Task<(bool Success, string? ErrorMessage)> TryWriteToJsonFileAsync(
            this List<DiffEntry>? diffs,
            string filePath,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await diffs.WriteToJsonFileAsync(filePath, options, cancellationToken);
                return (true, null);
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or JsonException)
            {
                return (false, ex.Message);
            }
        }

        // Helper extension for safe string truncation
        private static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value[..maxLength] + "...";
        }
    }
}