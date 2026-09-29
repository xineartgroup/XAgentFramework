using System.Text.Json.Serialization;

namespace XAgentFramework
{
    public class ImageUrl
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
