using System.Text.Json.Serialization;

namespace XAgentFramework
{
    public class AgentInfo
    {
        public string URL { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string SystemPrompt { get; set; } = string.Empty;
    }
}