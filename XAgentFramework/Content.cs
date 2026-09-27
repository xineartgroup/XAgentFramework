using System.Text.Json.Serialization;

namespace XAgentFramework
{
    public class Content
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "User"; // "User" or "Model"

        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = [];
    }
}
