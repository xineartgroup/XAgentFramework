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
        public static async Task SaveChatListItemAsync(ChatListItem chatItem, string filePath)
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
        public static void SaveChatListItem(ChatListItem chatItem, string filePath)
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
        public static async Task<ChatListItem?> LoadChatListItemAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            using FileStream openStream = File.OpenRead(filePath);
            ChatListItem? chatItem = await JsonSerializer.DeserializeAsync<ChatListItem>(openStream, JsonOptions);

            if (chatItem != null)
            {
                // Re-instantiate the runtime Client if URL and Key are available
                if (!string.IsNullOrEmpty(chatItem.URL) && !string.IsNullOrEmpty(chatItem.Key))
                {
                    chatItem.Client = new GeminiClient(chatItem.URL, chatItem.Key, chatItem.SystemPrompt);
                }
            }

            return chatItem;
        }

        /// <summary>
        /// Synchronous version to load a ChatListItem from disk.
        /// </summary>
        public static ChatListItem? LoadChatListItem(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            string json = File.ReadAllText(filePath);
            ChatListItem? chatItem = JsonSerializer.Deserialize<ChatListItem>(json, JsonOptions);

            if (chatItem != null)
            {
                // Re-instantiate the runtime Client if URL and Key are available
                if (!string.IsNullOrEmpty(chatItem.URL) && !string.IsNullOrEmpty(chatItem.Key))
                {
                    chatItem.Client = new GeminiClient(chatItem.URL, chatItem.Key, chatItem.SystemPrompt);
                }
            }

            return chatItem;
        }
    }
}