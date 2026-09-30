using System.Buffers.Text;
using System.Configuration;
using System.Reflection;

namespace XAgentFramework
{
    public partial class ChatForm : Form
    {
        private static readonly List<string> filePaths = [];

        private CancellationTokenSource? cts;

        public ChatForm()
        {
            InitializeComponent();
            chatList1.SelectionChanged += ChatList1_SelectionChanged;
            chatList1.SelectionHover += ChatList1_SelectionHover;
            chatList1.SaveRequested += ChatList1_SaveRequested;
            chatList1.UpdateRequested += ChatList1_ItemUpdated;
            chatList1.DeleteRequested += ChatList1_ItemDeleted;
            chatView1.DeleteRequested += ChatView1_DeleteRequested;
        }

        public static void AgentUrlMap()
        {
            var keys = ConfigurationManager.AppSettings.AllKeys;

            IEnumerable<string?> nameKeys = keys.Where(k => k != null && k.StartsWith("Model:") && k.EndsWith(":Name"));

            foreach (var nameKey in nameKeys)
            {
                string agentName = ConfigurationManager.AppSettings[nameKey] ?? "";

                if (nameKey != null)
                {
                    string urlKey = nameKey.Replace(":Name", ":Url");
                    string groupKey = nameKey.Replace(":Name", ":Group");
                    string url = ConfigurationManager.AppSettings[urlKey] ?? "";
                    string groupName = ConfigurationManager.AppSettings[groupKey] ?? "";
                    string key = GetAPIKey(groupName);

                    if (!string.IsNullOrEmpty(agentName) && !string.IsNullOrEmpty(url))
                    {
                        AgentInfo agentInfo = new()
                        {
                            Name = agentName,
                            URL = url,
                            Key = key,
                        };
                        LLMClientFactory.AgentsModelMap[agentName] = agentInfo;
                    }

                    LLMClientFactory.AddNameKey(agentName, groupName);
                }
            }
        }

        private async Task LoadAllAgentChatsAsync()
        {
            chatList1.ClearItems();

            List<string> filesToLoad = [];
            string manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "agent_manifest.txt");

            if (File.Exists(manifestPath))
            {
                filesToLoad = [.. File.ReadLines(manifestPath)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(relativePath => Path.Combine(Directory.GetCurrentDirectory(), relativePath))
                    .Where(File.Exists)];
            }
            else
            {
                string agentDir = Path.Combine(Directory.GetCurrentDirectory(), "agent_messages");
                if (Directory.Exists(agentDir))
                {
                    filesToLoad = [.. Directory.GetFiles(agentDir, "*.json")];
                }
            }

            if (filesToLoad.Count == 0) return;

            Task<ChatItem?>[] loadTasks = [.. filesToLoad.Select(file => ChatStorageService.LoadChatListItemAsync(file))];

            ChatItem?[] loadedItems = await Task.WhenAll(loadTasks);

            for (int i = 0; i < loadedItems.Length; i++)
            {
                ChatItem? chatItem = loadedItems[i];
                if (chatItem == null) continue;

                List<Message> messages = chatItem.Messages;
                string agentName = chatItem.Name;
                string agentPrompt = chatItem.AgentInfo.SystemPrompt;
                string baseUrl = chatItem.AgentInfo.URL;

                int result = chatList1.AddItem(new ChatItem(agentName, chatItem.AgentInfo.Name, baseUrl, chatItem.AgentInfo.Key, agentPrompt));
                if (result < 0)
                {
                    MessageBox.Show($"Ensure that '{agentName}' details are unique and correct.", "Failed to Add Agent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    chatList1.SetMessages(result, messages);

                    ChatItem? item = chatList1.GetAllItems()[result];
                    item.Client = LLMClientFactory.GetClient(chatItem.AgentInfo.Name, baseUrl, chatItem.AgentInfo.Key, agentPrompt);
                    item.Client?.UpdateHistory(GetHistory(messages));
                    LLMClientFactory.AgentsNameMap.Add(agentName, chatItem.AgentInfo.Name);
                }
            }

            if (chatList1.ItemCount() > 0)
            {
                chatList1.SelectedIndex = 0;
                ChatItem? firstItem = chatList1.GetSelectedItem();
                if (firstItem != null)
                {
                    chatView1.RenderMessages(firstItem.Messages);
                }
            }
        }

        private static string GetAPIKey(string apiGroup)
        {
            if (!File.Exists($"api_keys\\{apiGroup}.txt"))
            {
                return string.Empty;
            }
            return File.ReadAllText($"api_keys\\{apiGroup}.txt");
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "Loading...";

            AgentUrlMap();

            cboAgents.Items.Add("<-- Select an Agent -->");
            foreach (var kvp in LLMClientFactory.AgentsModelMap)
            {
                cboAgents.Items.Add(kvp.Key);
            }

            cboAgents.SelectedIndex = 0;

            await LoadAllAgentChatsAsync();

            btnCancelAttachment.Enabled = false;

            lblStatus.Text = "Ready";
        }

        private void CboAgents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboAgents.SelectedIndex > 0 && cboAgents.Items.Count > cboAgents.SelectedIndex && cboAgents.Items[cboAgents.SelectedIndex] is string baseName)
            {
                string baseUrl = LLMClientFactory.AgentsModelMap.TryGetValue(baseName, out AgentInfo? value) ? value.URL : string.Empty;
                NameForm nameForm = new()
                {
                    StartPosition = FormStartPosition.CenterParent,
                    AgentName = chatList1.GetUniqueName(baseName),
                };
                if (nameForm.ShowDialog() == DialogResult.OK)
                {
                    AgentInfo? agentInfo = LLMClientFactory.AgentsModelMap.TryGetValue(baseName, out AgentInfo? val) ? val : null;
                    if (agentInfo != null)
                    {
                        string agentName = nameForm.AgentName;
                        string agentPrompt = nameForm.AgentPrompt;
                        ILLMClient? client = LLMClientFactory.GetClient(agentInfo.Name, baseUrl, agentInfo.Key, agentPrompt);

                        if (client != null)
                        {
                            int result = chatList1.AddItem(new ChatItem(agentName, agentInfo.Name, baseUrl, agentInfo.Key, agentPrompt));
                            if (result >= 0)
                            {
                                chatList1.SetText(agentName);
                                LLMClientFactory.AgentsNameMap.Add(agentName, agentInfo.Name);
                            }
                            else
                            {
                                MessageBox.Show($"Ensure that '{agentName}' details are unique and correct.", "Failed to Add Agent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            chatView1.RenderMessages([]);
                        }
                    }
                }
                cboAgents.SelectedIndex = 0;
            }
        }

        private void BtnLoadFromFile_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Loading...";

            OpenFileDialog openFileDialog = new()
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Select an Agent Configuration File"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ChatStorageService.LoadChatListItemAsync(openFileDialog.FileName).ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        MessageBox.Show($"Error loading agent configuration: {task.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    ChatItem chatItem = task.Result ?? new ChatItem();
                    List<Message> messages = chatItem.Messages;
                    string agentName = chatItem.Name;
                    string agentPrompt = chatItem.AgentInfo.SystemPrompt;
                    string baseUrl = chatItem.AgentInfo.URL;
                    int result = chatList1.AddItem(new ChatItem(agentName, chatItem.AgentInfo.Name, baseUrl, chatItem.AgentInfo.Key, agentPrompt));
                    if (result < 0)
                    {
                        MessageBox.Show($"Ensure that '{agentName}' details are unique and correct.", "Failed to Add Agent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        chatList1.SetSelectedMessages(messages);
                        chatView1.RenderMessages(messages);

                        ChatItem? selectedItem = chatList1.GetSelectedItem();
                        selectedItem?.Client = LLMClientFactory.GetClient(chatItem.AgentInfo.Name, baseUrl, chatItem.AgentInfo.Key, agentPrompt);
                        selectedItem?.Client?.UpdateHistory(GetHistory(messages));
                        chatList1.SetSelectedItem(selectedItem);
                        LLMClientFactory.AgentsNameMap.Add(agentName, chatItem.AgentInfo.Name);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }

            lblStatus.Text = "Ready";
        }

        private static List<Content> GetHistory(List<Message> messages)
        {
            List<Content> history = [];
            foreach (var message in messages)
            {
                Content content = new()
                {
                    Role = message.Sender == "Me" ? "user" : "model",
                    Parts = [new Part { Text = message.Content }]
                };
                history.Add(content);
            }
            return history;
        }

        private void ChatList1_SelectionChanged(object? sender, ChatItemEventArgs e)
        {
            if (e.NewItem != null)
            {
                chatView1.RenderMessages(e.NewItem.Messages);
                chatList1.SetSelectedMessages(e.NewItem.Messages);
                Text = $"LLM Chat - {e.NewItem.Name}";
            }
            else
            {
                chatView1.RenderMessages([]);
                chatList1.SetSelectedMessages([]);
            }
        }

        private void ChatList1_SelectionHover(object? sender, ChatItemEventArgs e)
        {
            lblStatus.Text = e.NewItem != null ? $"[{e.NewItem.Name}]" : "Ready";
        }

        private void ChatList1_SaveRequested(object? sender, ChatItemEventArgs e)
        {
            MessageBox.Show($"'{e.NewItem?.Name}' has been saved.", "Agent Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ChatList1_ItemUpdated(object? sender, ChatItemEventArgs e)
        {
            string oldFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agent_messages", $"{e.OldItem?.Name}.json");
            string newFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agent_messages", $"{e.NewItem?.Name}.json");
            if (File.Exists(oldFilePath))
            {
                try
                {
                    File.Move(oldFilePath, newFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to rename file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ChatList1_ItemDeleted(object? sender, ChatItemEventArgs e)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agent_messages", $"{e.NewItem?.Name}.json");
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ChatView1_DeleteRequested(object? sender, ChatMessageEventArgs e)
        {
            MessageBox.Show($"Message '{e.DeletedMessage?.Content}' has been deleted.", "Message Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ChatItem? item = chatList1.GetSelectedItem();
            item?.Client?.UpdateHistory(GetHistory(e.CurrentMessages));
        }

        private void BtnAttach_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Attaching...";

            OpenFileDialog openFileDialog = new()
            {
                Filter = "All Files (*.*)|*.*|Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|PDF Files (*.pdf)|*.pdf",
                Title = "Select Files",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePaths.AddRange(openFileDialog.FileNames);
                lblAttachmentStatus.Text += $"[Attached {openFileDialog.FileNames.Length} file(s)] ";
                btnCancelAttachment.Enabled = true;
            }

            lblStatus.Text = "Ready";
        }

        private void BtnCancelAttachment_Click(object sender, EventArgs e)
        {
            filePaths.Clear();
            lblAttachmentStatus.Text = "";
            btnCancelAttachment.Enabled = false;
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            btnSend.Image = Properties.Resources.stop;

            lblStatus.Text = "Thinking...";

            if (cts != null)
            {
                cts.Cancel();
            }
            else if (!string.IsNullOrWhiteSpace(txtPrompt.Text) || filePaths.Count > 0)
            {
                ChatItem? chatItem = chatList1.GetSelectedItem();
                ILLMClient? client = chatItem?.Client;

                if (client != null)
                {
                    Message request = new()
                    {
                        Sender = "Me",
                        Recipient = chatItem?.Name ?? "Agent",
                        Content = txtPrompt.Text,
                        Time = DateTime.Now,
                        FilePaths = [.. filePaths]
                    };

                    List<Message> messages = chatList1.GetSelectedItem()?.Messages ?? [];

                    cts?.Cancel();
                    cts = new CancellationTokenSource();

                    try
                    {

                        Answer answer = await client.Question(txtPrompt.Text, filePaths, cts.Token);

                        if (answer.Success)
                        {
                            messages.Add(request);
                            chatList1.SetSelectedMessages(messages);
                            txtPrompt.Text = "";
                        }

                        Message response = new()
                        {
                            Sender = chatItem?.Name ?? "Agent",
                            Recipient = "Me",
                            Content = answer.Text,
                            Time = DateTime.Now,
                        };

                        messages.Add(response);
                        chatList1.SetSelectedMessages(messages);

                        chatView1.RenderMessages(messages);
                    }
                    catch (OperationCanceledException)
                    {
                        MessageBox.Show("The request was canceled.", "Request Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        cts.Dispose();
                        cts = null;
                    }
                }
                else
                {
                    Message response = new()
                    {
                        Sender = chatItem?.Name ?? "Agent",
                        Recipient = "Me",
                        Content = "Please select an agent.",
                        Time = DateTime.Now,
                    };

                    List<Message> messages = chatList1.GetSelectedItem()?.Messages ?? [];
                    messages.Add(response);
                    chatList1.SetSelectedMessages(messages);

                    chatView1.RenderMessages(messages);
                }
            }

            filePaths.Clear();

            lblAttachmentStatus.Text = "";
            lblStatus.Text = "Ready";
            btnCancelAttachment.Enabled = false;

            btnSend.Image = Properties.Resources.send;
        }

        private void TxtPrompt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && (ModifierKeys & Keys.Shift) == 0)
            {
                e.Handled = true;
                BtnSend_Click(sender, EventArgs.Empty);
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            ChatItem? foundItem = chatList1.SearchItem(txtSearch.Text);
            if (foundItem != null)
            {
                chatList1.SelectItem(foundItem);
            }
        }

        private async void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;

            try
            {
                string currentDir = Directory.GetCurrentDirectory();
                string targetDir = Path.Combine(currentDir, "agent_messages");
                Directory.CreateDirectory(targetDir);

                List<ChatItem> items = chatList1.GetAllItems();
                List<string> manifestLines = [];
                List<Task> saveTasks = [];

                HashSet<string> expectedFiles = new(StringComparer.OrdinalIgnoreCase);

                foreach (ChatItem item in items)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.Name))
                        continue;

                    string relativePath = Path.Combine("agent_messages", $"{item.Name}.json");
                    string fullPath = Path.Combine(currentDir, relativePath);

                    expectedFiles.Add($"{item.Name}.json");

                    saveTasks.Add(ChatStorageService.SaveChatListItemAsync(item, fullPath));
                    manifestLines.Add(relativePath);
                }

                await Task.WhenAll(saveTasks);

                foreach (string existingFile in Directory.EnumerateFiles(targetDir))
                {
                    string fileName = Path.GetFileName(existingFile);

                    if (!expectedFiles.Contains(fileName))
                    {
                        try
                        {
                            File.Delete(existingFile);
                        }
                        catch
                        {
                            
                        }
                    }
                }

                string manifestPath = Path.Combine(currentDir, "agent_manifest.txt");
                await File.WriteAllLinesAsync(manifestPath, manifestLines);
            }
            catch
            {

            }
            finally
            {
                FormClosing -= ChatForm_FormClosing;
                Close();
            }
        }
    }
}
