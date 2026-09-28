using System.Text.Json;
using System.Text.Json.Serialization;

namespace XAgentFramework
{
    public static class ChatStorageService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Saves a complete ChatListItem (metadata and messages) to a JSON file.
        /// </summary>
        public static async Task SaveChatListItemAsync(ChatItem chatItem, string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using FileStream createStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(createStream, chatItem, JsonOptions);
        }

        /// <summary>
        /// Synchronous version for simple local file saves.
        /// </summary>
        public static void SaveChatListItem(ChatItem chatItem, string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(chatItem, JsonOptions);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads a ChatListItem from a JSON file and re-initializes its GeminiClient.
        /// </summary>
        public static async Task<ChatItem?> LoadChatListItemAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            using FileStream openStream = File.OpenRead(filePath);
            ChatItem? chatItem = await JsonSerializer.DeserializeAsync<ChatItem>(openStream, JsonOptions);

            if (chatItem != null)
            {
                // Re-instantiate the runtime Client if URL and Key are available
                if (!string.IsNullOrEmpty(chatItem.AgentInfo.URL) && !string.IsNullOrEmpty(chatItem.AgentInfo.Key))
                {
                    chatItem.Client = new GeminiClient(chatItem.AgentInfo.URL, chatItem.AgentInfo.Key, chatItem.AgentInfo.SystemPrompt);
                }
            }

            return chatItem;
        }

        /// <summary>
        /// Synchronous version to load a ChatListItem from disk.
        /// </summary>
        public static ChatItem? LoadChatListItem(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            string json = File.ReadAllText(filePath);
            ChatItem? chatItem = JsonSerializer.Deserialize<ChatItem>(json, JsonOptions);

            if (chatItem != null)
            {
                // Re-instantiate the runtime Client if URL and Key are available
                if (!string.IsNullOrEmpty(chatItem.AgentInfo.URL) && !string.IsNullOrEmpty(chatItem.AgentInfo.Key))
                {
                    chatItem.Client = new GeminiClient(chatItem.AgentInfo.URL, chatItem.AgentInfo.Key, chatItem.AgentInfo.SystemPrompt);
                }
            }

            return chatItem;
        }
    }
}