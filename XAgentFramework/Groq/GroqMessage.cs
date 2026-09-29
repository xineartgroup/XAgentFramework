using System.Text.Json.Serialization;

namespace XAgentFramework.Groq
{
    public class GroqMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Either a plain string (text-only turns) or a List&lt;GroqContentPart&gt;
        /// (multimodal turns). Serialized by <see cref="GroqContentConverter"/>.
        /// </summary>
        [JsonPropertyName("content")]
        [JsonConverter(typeof(GroqContentConverter))]
        public object? Content { get; set; }
    }
}
