using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altered.Core.Converters
{

    public class ObjectConverter : JsonConverter<object>
    {
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.String:
                    var s = reader.GetString()!;

                    // Handle UTC (ends with Z)
                    if (s.EndsWith("Z"))
                    {
                        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dtUtc))
                            return DateTime.SpecifyKind(dtUtc, DateTimeKind.Utc);
                        return s;
                    }
                    // Handle offset (contains + or -)
                    if (s.Contains('+') || s.Contains('-'))
                    {
                        if (DateTimeOffset.TryParse(s, out DateTimeOffset dto))
                            return dto;
                    }

                    // Plain date time without offset
                    if (DateTime.TryParse(s, out DateTime dt))
                        return dt;
                    return s;
                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out int i)) return i;
                    if (reader.TryGetInt64(out long l)) return l;
                    if (reader.TryGetDouble(out double d)) return d;
                    return reader.GetDecimal();
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                default:
                    using (var doc = JsonDocument.ParseValue(ref reader))
                        return doc.RootElement.Clone();
            }
        }

        public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            switch (value)
            {
                case DateTime dt:
                    string formatted;
                    if (dt.Kind == DateTimeKind.Utc)
                        formatted = dt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
                    else
                        formatted = dt.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
                    writer.WriteStringValue(formatted);
                    break;
                case DateTimeOffset dto:
                    if (dto.Offset == TimeSpan.Zero)
                        formatted = dto.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
                    else
                        formatted = dto.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
                    writer.WriteStringValue(formatted);
                    break;
                default:
                    JsonSerializer.Serialize(writer, value, value.GetType(), options);
                    break;
            }
        }
    }
}