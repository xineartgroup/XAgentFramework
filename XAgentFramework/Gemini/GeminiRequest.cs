using System.Text.Json.Serialization;

namespace XAgentFramework.Gemini
{
    public class GeminiRequest
    {
        [JsonPropertyName("system_instruction")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Content SystemInstruction { get; set; } = new Content();

        [JsonPropertyName("contents")]
        public List<Content> Contents { get; set; } = [];
    }
}
