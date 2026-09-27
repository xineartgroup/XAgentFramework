using System.Text.Json.Serialization;

namespace XAgentFramework
{
    public class ChatListItem
    {
        public string Name { get; set; } = string.Empty;

        public string URL { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string SystemPrompt { get; set; } = string.Empty;

        public List<Message> Messages { get; set; } = [];

        [JsonIgnore]
        public GeminiClient? Client = null;

        public ChatListItem()
        {
        }

        public ChatListItem(string name, string url, string key, string systemPrompt)
        {
            Name = name;
            URL = url;
            Key = key;
            SystemPrompt = systemPrompt;
            Client = new GeminiClient(url, key, systemPrompt);
        }
    }
}