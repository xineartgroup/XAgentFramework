using System.Configuration;

namespace XAgentFramework
{
    public partial class ChatForm : Form
    {
        private static readonly Dictionary<string, string> modelMap = [];

        private static readonly List<string> filePaths = [];

        private static readonly string geminiAPIKey = GetAPIKey();

        private CancellationTokenSource? cts;

        public ChatForm()
        {
            InitializeComponent();
            chatList1.SelectionChanged += ChatList1_SelectionChanged;
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
                    string url = ConfigurationManager.AppSettings[urlKey] ?? "";

                    if (!string.IsNullOrEmpty(modelName) && !string.IsNullOrEmpty(url))
                    {
                        modelMap[modelName] = url;
                    }
                }
            }
        }

        private static string GetAPIKey()
        {
            return File.ReadAllText("gemini_api_key.txt");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "Loading...";

            ModelUrlMap();

            cboModels.Items.Add("<-- Select an Agent -->");
            foreach (var kvp in modelMap)
            {
                cboModels.Items.Add(kvp.Key);
            }

            cboModels.SelectedIndex = 0;

            btnCancelAttachment.Enabled = false;

            lblStatus.Text = "Ready";
        }

        private void CboModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboModels.SelectedIndex > 0 && cboModels.Items.Count > cboModels.SelectedIndex && cboModels.Items[cboModels.SelectedIndex] is string baseName)
            {
                string baseUrl = modelMap.TryGetValue(baseName, out string? value) ? value : string.Empty;
                NameForm nameForm = new()
                {
                    StartPosition = FormStartPosition.CenterParent,
                    ModelName = chatList1.GetUniqueName(baseName),
                };
                if (nameForm.ShowDialog() == DialogResult.OK)
                {
                    string modelName = nameForm.ModelName;
                    string modelPrompt = nameForm.ModelPrompt;
                    GeminiClient? client = new(baseUrl, geminiAPIKey, modelPrompt);

                    if (client != null)
                    {
                        int result = chatList1.AddItem(new ChatListItem(modelName, baseUrl, geminiAPIKey, modelPrompt));
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
                ChatStorageService.LoadChatListItemAsync(openFileDialog.FileName).ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        MessageBox.Show($"Error loading model configuration: {task.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    ChatListItem chatItem = task.Result ?? new ChatListItem();
                    List<Message> messages = chatItem.Messages;
                    string modelName = chatItem.Name;
                    string modelPrompt = chatItem.SystemPrompt;
                    string baseUrl = chatItem.URL;
                    int result = chatList1.AddItem(new ChatListItem(modelName, baseUrl, geminiAPIKey, modelPrompt));
                    if (result < 0)
                    {
                        MessageBox.Show($"Ensure that model '{modelName}' details are unique and correct.", "Failed to Add Model", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        chatList1.SetSelectedMessages(messages);
                        chatView1.RenderMessages(messages);

                        ChatListItem? selectedItem = chatList1.GetSelectedItem();
                        selectedItem?.Client = new(baseUrl, geminiAPIKey, modelPrompt);
                        selectedItem?.Client?.UpdateHistory(GetHistory(messages));
                        chatList1.SetSelectedItem(selectedItem);
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

        private void ChatList1_SelectionChanged(object? sender, ChatList.SelectionChangedEventArgs e)
        {
            if (e.Item != null)
            {
                chatView1.RenderMessages(e.Item.Messages);
                chatList1.SetSelectedMessages(e.Item.Messages);
            }
            else
            {
                chatView1.RenderMessages([]);
                chatList1.SetSelectedMessages([]);
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
                ChatListItem? chatItem = chatList1.GetSelectedItem();
                GeminiClient? client = chatItem?.Client;

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
    }
}
