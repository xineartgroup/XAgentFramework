using System.Configuration;
using System.Reflection;
using System.Xml.Linq;

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
        }

        public static void ModelUrlMap()
        {
            var keys = ConfigurationManager.AppSettings.AllKeys;

            IEnumerable<string?> nameKeys = keys.Where(k => k != null && k.StartsWith("Model:") && k.EndsWith(":Name"));

            foreach (var nameKey in nameKeys)
            {
                string modelName = ConfigurationManager.AppSettings[nameKey] ?? "";

                if (nameKey != null)
                {
                    string urlKey = nameKey.Replace(":Name", ":Url");
                    string groupKey = nameKey.Replace(":Name", ":Group");
                    string url = ConfigurationManager.AppSettings[urlKey] ?? "";
                    string groupName = ConfigurationManager.AppSettings[groupKey] ?? "";
                    string key = GetAPIKey(groupName);

                    if (!string.IsNullOrEmpty(modelName) && !string.IsNullOrEmpty(url))
                    {
                        AgentInfo agentInfo = new()
                        {
                            Name = modelName,
                            URL = url,
                            Key = key,
                        };
                        LLMClientFactory.ModelMap[modelName] = agentInfo;
                    }

                    LLMClientFactory.AddNameKey(modelName, groupName);
                }
            }
        }

        private void LoadAgentChat(string fileName)
        {
            ChatStorageService.LoadChatListItemAsync(fileName).ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    MessageBox.Show($"Error loading model configuration: {task.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                ChatItem chatItem = task.Result ?? new ChatItem();
                List<Message> messages = chatItem.Messages;
                string modelName = chatItem.Name;
                string modelPrompt = chatItem.AgentInfo.SystemPrompt;
                string baseUrl = chatItem.AgentInfo.URL;
                int result = chatList1.AddItem(new ChatItem(modelName, chatItem.AgentInfo.Name, baseUrl, chatItem.AgentInfo.Key, modelPrompt));
                if (result < 0)
                {
                    MessageBox.Show($"Ensure that model '{modelName}' details are unique and correct.", "Failed to Add Model", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    chatList1.SetSelectedMessages(messages);
                    chatView1.RenderMessages(messages);

                    ChatItem? selectedItem = chatList1.GetSelectedItem();
                    selectedItem?.Client = LLMClientFactory.GetClient(chatItem.AgentInfo.Name, baseUrl, chatItem.AgentInfo.Key, modelPrompt);
                    selectedItem?.Client?.UpdateHistory(GetHistory(messages));
                    chatList1.SetSelectedItem(selectedItem);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static string GetAPIKey(string apiGroup)
        {
            if (!File.Exists($"api_keys\\{apiGroup}.txt"))
            {
                return string.Empty;
            }
            return File.ReadAllText($"api_keys\\{apiGroup}.txt");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "Loading...";

            ModelUrlMap();

            cboModels.Items.Add("<-- Select an Agent -->");
            foreach (var kvp in LLMClientFactory.ModelMap)
            {
                cboModels.Items.Add(kvp.Key);
            }

            cboModels.SelectedIndex = 0;

            string[] agentFiles = Directory.GetFiles("agent_messages", "*.json");
            foreach (string agentFile in agentFiles)
            {
                LoadAgentChat(agentFile);
            }

            btnCancelAttachment.Enabled = false;

            lblStatus.Text = "Ready";
        }

        private void CboModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboModels.SelectedIndex > 0 && cboModels.Items.Count > cboModels.SelectedIndex && cboModels.Items[cboModels.SelectedIndex] is string baseName)
            {
                string baseUrl = LLMClientFactory.ModelMap.TryGetValue(baseName, out AgentInfo? value) ? value.URL : string.Empty;
                NameForm nameForm = new()
                {
                    StartPosition = FormStartPosition.CenterParent,
                    ModelName = chatList1.GetUniqueName(baseName),
                };
                if (nameForm.ShowDialog() == DialogResult.OK)
                {
                    AgentInfo? agentInfo = LLMClientFactory.ModelMap.TryGetValue(baseName, out AgentInfo? val) ? val : null;
                    if (agentInfo != null)
                    {
                        string modelName = nameForm.ModelName;
                        string modelPrompt = nameForm.ModelPrompt;
                        ILLMClient? client = LLMClientFactory.GetClient(agentInfo.Name, baseUrl, agentInfo.Key, modelPrompt);

                        if (client != null)
                        {
                            int result = chatList1.AddItem(new ChatItem(modelName, agentInfo.Name, baseUrl, agentInfo.Key, modelPrompt));
                            if (result >= 0)
                            {
                                chatList1.SetText(modelName);
                            }
                            else
                            {
                                MessageBox.Show($"Ensure that model '{modelName}' details are unique and correct.", "Failed to Add Model", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            chatView1.RenderMessages([]);
                        }
                    }
                }
                cboModels.SelectedIndex = 0;
            }
        }

        private void BtnLoadFromFile_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Loading...";

            OpenFileDialog openFileDialog = new()
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Select a Model Configuration File"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadAgentChat(openFileDialog.FileName);
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

        private void ChatList1_SelectionChanged(object? sender, ChatList.ItemEventArgs e)
        {
            if (e.NewItem != null)
            {
                chatView1.RenderMessages(e.NewItem.Messages);
                chatList1.SetSelectedMessages(e.NewItem.Messages);
            }
            else
            {
                chatView1.RenderMessages([]);
                chatList1.SetSelectedMessages([]);
            }
        }

        private void ChatList1_SelectionHover(object? sender, ChatList.ItemEventArgs e)
        {
            lblStatus.Text = e.NewItem != null ? $"[{e.NewItem.Name}]" : "Ready";
        }

        private void ChatList1_SaveRequested(object? sender, ChatList.ItemEventArgs e)
        {
            MessageBox.Show($"'{e.NewItem?.Name}' has been saved.", "Model Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ChatList1_ItemUpdated(object? sender, ChatList.ItemEventArgs e)
        {
            //MessageBox.Show($"'{e.OldItem?.Name}' has been updated to '{e.NewItem?.Name}'.", "Model Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void ChatList1_ItemDeleted(object? sender, ChatList.ItemEventArgs e)
        {
            //MessageBox.Show($"'{e.NewItem?.Name}' has been deleted.", "Model Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        Recipient = chatItem?.Name ?? "Model",
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
                            Sender = chatItem?.Name ?? "Model",
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
                        Sender = chatItem?.Name ?? "Model",
                        Recipient = "Me",
                        Content = "Please select a model.",
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

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (ChatItem item in chatList1.GetAllItems())
            {
                if (item != null)
                {
                    ChatStorageService.SaveChatListItemAsync(item, $"agent_messages\\{item.Name}.json");
                }
            }
        }
    }
}
