using System.Text.Json.Serialization;

namespace XAgentFramework
{
    public class ChatItem
    {
        public string Name { get; set; } = string.Empty;

        public AgentInfo AgentInfo { get; set; } = new AgentInfo();

        public List<Message> Messages { get; set; } = [];

        [JsonIgnore]
        public GeminiClient? Client = null;

        public ChatItem()
        {
        }

        public ChatItem(string name, string url, string key, string systemPrompt)
        {
            Name = name;
            AgentInfo = new AgentInfo
            {
                URL = url,
                Key = key,
                SystemPrompt = systemPrompt
            };
            Client = new GeminiClient(url, key, systemPrompt);
        }
    }
}