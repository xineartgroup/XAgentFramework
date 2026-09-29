namespace XAgentFramework
{
    public static class LLMClientFactory
    {
        private static readonly Dictionary<string, string> nameKeys = [];

        public static readonly Dictionary<string, AgentInfo> ModelMap = [];

        public static void AddNameKey(string name, string key)
        {
            nameKeys[name] = key;
        }

        public static ILLMClient? GetClient(string name, string baseUrl, string key, string modelPrompt)
        {
            string groupName = nameKeys.TryGetValue(name, out string? value) ? value : "";
            if (groupName == "gemini_api_key")
            {
                return new Gemini.GeminiClient(baseUrl, key, modelPrompt);
            }
            else if (groupName == "groq_api_key")
            {
                return new Groq.GroqClient(baseUrl, key, modelPrompt, name);
            }
            else
            {
                return null;
            }
        }
    }
}
