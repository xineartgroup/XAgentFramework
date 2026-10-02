using System.Text.Json.Serialization;

namespace XAgentFramework
{
    public class ChatItem
    {
        public string Name { get; set; } = string.Empty;

        public AgentInfo AgentInfo { get; set; } = new AgentInfo();

        public List<Message> Messages { get; set; } = [];

        [JsonIgnore]
        public IAgentClient? Client = null;

        public ChatItem()
        {
        }

        public ChatItem(string name, AgentInfo agentInfo, IAgentClient? client)
        {
            Name = name;
            AgentInfo = agentInfo;
            Client = client;
        }

        public ChatItem(ChatItem? other)
        {
            Name = other?.Name ?? string.Empty;
            AgentInfo = other?.AgentInfo ?? new AgentInfo();
            Client = ClientFactory.GetClient(AgentInfo.Model, AgentInfo.URL, AgentInfo.Key, AgentInfo.Prompt);
        }
    }
}