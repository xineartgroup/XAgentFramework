namespace XAgentFramework
{
    public static class ClientFactory
    {
        private static readonly Dictionary<string, string> nameKeyMap = [];

        public static readonly Dictionary<string, AgentInfo> ModelsMap = [];

        public static readonly Dictionary<string, AgentInfo> AgentsMap = [];

        public static void AddNameKey(string name, string key)
        {
            nameKeyMap[name] = key;
        }

        public static IAgentClient? GetClient(string name, string baseUrl, string key, string modelPrompt)
        {
            string groupName = nameKeyMap.TryGetValue(name, out string? value) ? value : "";
            if (groupName.StartsWith("gemini_api_key"))
            {
                return new Gemini.GeminiClient(baseUrl, key, modelPrompt);
            }
            else if (groupName.StartsWith("groq_api_key"))
            {
                return new Groq.GroqClient(baseUrl, key, modelPrompt, name);
            }
            else if (groupName.StartsWith("agenx_api_key"))
            {
                return new Agenx.AgenxClient();
            }
            else
            {
                return null;
            }
        }

        public static string GetOrchestrationPrompt(string name)
        {
            string prompt = $"\r\nYou are an orchestrator called {name} that can use other agents to complete tasks." +
                "\r\nTo accomplish this, you can utilize the following agents.\r\n" +
                "\r\nAvailable agents and what they can do (their system prompt):";

            foreach (KeyValuePair<string, AgentInfo> kvp in AgentsMap)
            {
                prompt += $"\r\n  [{kvp.Key}]: [{kvp.Value.Prompt}]";
            }

            prompt += "\r\n\r\nFirst, determine if you need to use an agent to complete the task." +
                "\r\nIf so, write your answer as a prompt in the format:" +
                $"\r\n  [Agent Name]: prompt [{name}]" +
                "\r\nYour answer will be sent to the appropriate agent that will send a follow-up prompt in the format." +
                $"\r\n  [{name}]: answer" +
                $"\r\nIf you see a prompt from an agent, display the answer or use the answer appropriately." +
                "\r\n\r\nIf you don't need to use an agent, just answer the question directly.";

            return prompt;
        }
    }
}
