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
                if (kvp.Key != name && kvp.Value.IsOrchestrator == false)
                {
                    prompt += $"\r\n  [{kvp.Key}]: [{kvp.Value.Prompt}]";
                }
            }

            prompt += "\r\n\r\nFirst, determine if you need to use an agent to complete the task." +
                "\r\nIf so, write your answer as a prompt in the format:" +
                $"\r\n  [Agent Name]: prompt [{name}]" +
                $"\r\nYour answer will be sent to the appropriate agent." +
                $"\r\nYou will be sent a follow-up prompt in the format:" +
                $"\r\n[YOUR TASK]:\r\noriginal task" +
                $"\r\n[TASK 1]:\r\nprompt" +
                $"\r\n[TASK 1]:\r\nanswer" +
                $"\r\n[TASK 2]:\r\nprompt" +
                $"\r\n[TASK 2]:\r\nanswer" +
                $"\r\nIf you see a prompt in the follow-up format, use the answer to send follow-up tasks." +
                $"\r\n\r\nIf you don't need to use an agent or you have no more tasks, answer the question." +
                $"\r\n\r\nIf you are asked for folders or non-text based files (e.g., images, pdf files), provide the following template as a task." +
                $"\r\n[LIST OF FILES] [comma-separated file paths]." +
                $"\r\nThe list will be used to attach the specified files to the prompt.";

            return prompt;
        }
    }
}
