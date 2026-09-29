using System.Text.Json.Serialization;

namespace XAgentFramework
{
    public class ContentPart
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Text { get; set; }

        [JsonPropertyName("image_url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ImageUrl? ImageUrl { get; set; }
    }
}
