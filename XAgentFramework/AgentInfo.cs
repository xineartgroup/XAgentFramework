namespace XAgentFramework
{
    public class AgentInfo
    {
        public string Model { get; set; } = string.Empty;

        public string URL { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Prompt { get; set; } = string.Empty;

        public bool IsOrchestrator { get; set; } = false;

        public bool AutoPrompt { get; set; } = false;
    }
}