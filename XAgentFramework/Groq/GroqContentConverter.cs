using System.Text.Json;
using System.Text.Json.Serialization;

namespace XAgentFramework.Groq
{
    /// <summary>
    /// Serializes <see cref="GroqMessage.Content"/> as either a JSON string
    /// or a JSON array of content parts, depending on its runtime type.
    /// </summary>
    public class GroqContentConverter : JsonConverter<object>
    {
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return doc.RootElement.Clone();
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    writer.WriteNullValue();
                    break;

                case string s:
                    writer.WriteStringValue(s);
                    break;

                case IEnumerable<ContentPart> parts:
                    writer.WriteStartArray();
                    foreach (var part in parts)
                    {
                        JsonSerializer.Serialize(writer, part, options);
                    }
                    writer.WriteEndArray();
                    break;

                default:
                    JsonSerializer.Serialize(writer, value, options);
                    break;
            }
        }
    }
}
