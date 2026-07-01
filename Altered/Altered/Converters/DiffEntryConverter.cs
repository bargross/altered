using Altered.Main;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altered.Converters;

public class DiffEntryConverter : JsonConverter<DiffEntry>
{
    public override DiffEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var entry = new DiffEntry();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propName = reader.GetString();
            reader.Read();

            switch (propName)
            {
                case "PropertyName":
                    entry.PropertyName = reader.GetString() ?? string.Empty;
                    break;
                case "OldValue":
                    entry.OldValue = JsonSerializer.Deserialize<object>(ref reader, options);
                    break;
                case "NewValue":
                    entry.NewValue = JsonSerializer.Deserialize<object>(ref reader, options);
                    break;
                case "NewValueTypeHint":
                    entry.NewValueTypeHint = reader.GetString();
                    break;
            }
        }
        return entry;
    }

    public override void Write(Utf8JsonWriter writer, DiffEntry value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // 1. Write PropertyName (not "Path")
        writer.WriteString("PropertyName", value.PropertyName);

        // 2. Write the Type Hint (if NewValue is a problematic type)
        if (value.NewValue != null)
        {
            Type type = value.NewValue.GetType();
            if (IsProblematicType(type))
            {
                writer.WriteString("NewValueTypeHint", type.AssemblyQualifiedName);
            }
        }

        // 3. Write OldValue
        writer.WritePropertyName("OldValue");
        JsonSerializer.Serialize(writer, value.OldValue, typeof(object), options);

        // 4. Write NewValue
        writer.WritePropertyName("NewValue");
        JsonSerializer.Serialize(writer, value.NewValue, typeof(object), options);

        writer.WriteEndObject();
    }

    private static bool IsProblematicType(Type type)
    {
        return type.IsPrimitive
            || type == typeof(decimal)
            || type == typeof(float)
            || type == typeof(double)
            || type.IsEnum;
    }
}