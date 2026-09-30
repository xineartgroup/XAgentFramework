using System.ComponentModel;

namespace XAgentFramework
{
    public partial class ChatList : UserControl
    {
        private int selectedIndex = -1;
        private int hoverIndex = -1;
        private const int LABEL_HEIGHT = 50;
        private readonly List<ChatItem> items = [];

        private ContextMenuStrip contextMenu = null!;
        private ToolStripMenuItem saveMenuItem = null!;
        private ToolStripMenuItem deleteMenuItem = null!;
        private ToolStripMenuItem settingsMenuItem = null!;

        [Category("Behavior")]
        [Description("Gets or sets the index of the currently selected item.")]
        [DefaultValue(-1)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex != value)
                {
                    ChatItem? oldItem = selectedIndex >= 0 && selectedIndex < items.Count ? items[selectedIndex] : null;
                    selectedIndex = value;
                    ChatItem? newItem = selectedIndex >= 0 && selectedIndex < items.Count ? items[selectedIndex] : null;

                    SelectionChanged?.Invoke(this, new ItemEventArgs(oldItem, newItem));
                    panel1.Invalidate();
                }
            }
        }

        public event EventHandler<ItemEventArgs>? SelectionHover = null;

        public event EventHandler<ItemEventArgs>? SelectionChanged = null;

        public event EventHandler<ItemEventArgs>? SaveRequested = null;

        public event EventHandler<ItemEventArgs>? UpdateRequested = null;

        public event EventHandler<ItemEventArgs>? DeleteRequested = null;

        public class ItemEventArgs(ChatItem? oldItem, ChatItem? newItem) : EventArgs
        {
            public ChatItem? OldItem { get; } = oldItem;

            public ChatItem? NewItem { get; } = newItem;
        }

        public ChatList()
        {
            InitializeComponent();

            InitializeContextMenu();

            // Double buffer panel1 to eliminate flickers during paint/scroll
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, panel1, [true]);

            // Re-adjust panel widths when resized
            panelContainer.Resize += (s, e) =>
            {
                panel1.Width = panelContainer.ClientSize.Width;
                UpdatePanelHeight();
            };
        }

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();

            saveMenuItem = new ToolStripMenuItem("Save");
            deleteMenuItem = new ToolStripMenuItem("Delete");
            settingsMenuItem = new ToolStripMenuItem("Settings");

            saveMenuItem.Click += SaveMenuItem_Click;
            deleteMenuItem.Click += DeleteMenuItem_Click;
            settingsMenuItem.Click += SettingsMenuItem_Click;

            contextMenu.Items.AddRange(
            [
                saveMenuItem,
                deleteMenuItem,
                new ToolStripSeparator(),
                settingsMenuItem
            ]);
        }

        private static async Task SaveItem(ChatItem item)
        {
            using SaveFileDialog saveFileDialog = new()
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                FileName = $"{item.Name}_chat.json",
                Title = "Save Chat History"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await ChatStorageService.SaveChatListItemAsync(item, saveFileDialog.FileName);
                    MessageBox.Show("Chat saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save chat: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void SaveMenuItem_Click(object? sender, EventArgs e)
        {
            if (GetSelectedItem() is { } item)
            {
                await SaveItem(item);
                SaveRequested?.Invoke(this, new ItemEventArgs(null, item));
            }
        }

        private void DeleteMenuItem_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this chat?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (SelectedIndex >= 0 && GetSelectedItem() is { } item)
                {
                    DeleteRequested?.Invoke(this, new ItemEventArgs(item, null));
                    RemoveAt(SelectedIndex);
                }
            }
        }

        private void SettingsMenuItem_Click(object? sender, EventArgs e)
        {
            if (GetSelectedItem() is { } item)
            {
                SettingsForm settingsForm = new()
                {
                    SelectedItem = item,
                    StartPosition = FormStartPosition.CenterParent
                };

                ChatItem? oldItem = selectedIndex >= 0 && selectedIndex < items.Count ? new ChatItem(items[selectedIndex]) : null;
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    SetSelectedItem(settingsForm.SelectedItem);
                    UpdateRequested?.Invoke(this, new ItemEventArgs(oldItem, settingsForm.SelectedItem));
                }
            }
        }

        private void UpdatePanelHeight()
        {
            panel1.SuspendLayout();
            panel1.Width = panelContainer.ClientSize.Width;
            panel1.Height = Math.Max(items.Count * LABEL_HEIGHT, panelContainer.ClientSize.Width > 0 ? panelContainer.ClientSize.Height : 0);
            panel1.ResumeLayout();
        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            bool match = false;
            for (int i = 0; i < items.Count; i++)
            {
                if (e.X >= 0 && e.X <= panel1.Width && e.Y >= i * LABEL_HEIGHT && e.Y < (i + 1) * LABEL_HEIGHT)
                {
                    match = true;
                    if (hoverIndex != i)
                    {
                        ChatItem? oldItem = hoverIndex >= 0 && hoverIndex < items.Count ? items[hoverIndex] : null;
                        hoverIndex = i;
                        SelectionHover?.Invoke(this, new ItemEventArgs(oldItem, items[i]));
                        panel1.Invalidate();
                    }
                    break;
                }
            }

            if (!match && hoverIndex >= 0)
            {
                ChatItem? oldItem = items[hoverIndex];
                hoverIndex = -1;
                SelectionHover?.Invoke(this, new ItemEventArgs(oldItem, null));
                panel1.Invalidate();
            }
        }

        private void Panel1_MouseLeave(object sender, EventArgs e)
        {
            hoverIndex = -1;
            panel1.Invalidate();
        }

        private void Panel1_MouseClick(object sender, MouseEventArgs e)
        {
            bool match = false;
            for (int i = 0; i < items.Count; i++)
            {
                if (e.X >= 0 && e.X <= panel1.Width && e.Y >= i * LABEL_HEIGHT && e.Y < (i + 1) * LABEL_HEIGHT)
                {
                    match = true;
                    SelectedIndex = i;

                    if (e.Button == MouseButtons.Right)
                    {
                        contextMenu.Show(panel1, e.Location);
                    }
                    break;
                }
            }

            if (!match)
            {
                SelectedIndex = -1;
            }
        }

        private void Panel1_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Font font = new("Arial", 10, FontStyle.Bold);
            using Font font1 = new("Arial", 8);
            using Font avatarFont = new("Arial", 9, FontStyle.Bold);

            using StringFormat avatarFormat = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            for (int i = 0; i < items.Count; i++)
            {
                Rectangle rect = new(0, i * LABEL_HEIGHT, panel1.Width, LABEL_HEIGHT);
                if (!e.ClipRectangle.IntersectsWith(rect))
                {
                    continue;
                }

                Message? lastMessage = items[i].Messages.LastOrDefault();
                string name = items[i].Name ?? "";
                string txtUsername = name;
                string txtMessage = "No messages...";
                bool isRead = true;

                if (lastMessage != null)
                {
                    isRead = lastMessage.IsRead;
                    txtMessage = lastMessage.Content.Length > 10
                        ? string.Concat(lastMessage.Content.AsSpan(0, 10), "...")
                        : lastMessage.Content;
                    txtMessage += "    " + lastMessage.Time.ToString("dd-MM-yyyy");
                }

                Brush textBrush;
                Brush subTextBrush;

                if (i == hoverIndex)
                {
                    g.FillRectangle(Brushes.LightGray, rect);
                    textBrush = Brushes.White;
                    subTextBrush = isRead ? Brushes.White : Brushes.Green;
                }
                else if (i == SelectedIndex)
                {
                    g.FillRectangle(Brushes.DarkGray, rect);
                    textBrush = Brushes.White;
                    subTextBrush = isRead ? Brushes.White : Brushes.Green;
                }
                else
                {
                    g.FillRectangle(Brushes.White, rect);
                    textBrush = Brushes.Black;
                    subTextBrush = isRead ? Brushes.DarkGray : Brushes.Green;
                }

                int avatarPadding = 5;
                int avatarSize = LABEL_HEIGHT - (avatarPadding * 2);
                Rectangle avatarRect = new(avatarPadding, (i * LABEL_HEIGHT) + avatarPadding, avatarSize, avatarSize);

                using (SolidBrush avatarBgBrush = new(Color.SteelBlue))
                {
                    g.FillEllipse(avatarBgBrush, avatarRect);
                }

                string initials = name.Length >= 2
                    ? name[..2].ToUpper()
                    : name.ToUpper();

                g.DrawString(initials, avatarFont, Brushes.White, avatarRect, avatarFormat);

                int textX = avatarRect.Right + 10;
                Point point = new(textX, i * LABEL_HEIGHT + 5);
                Point point1 = new(textX, i * LABEL_HEIGHT + 25);

                g.DrawString(txtUsername, font, textBrush, point);
                g.DrawString(txtMessage, font1, subTextBrush, point1);
            }
        }

        public int AddItem(ChatItem item)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name == item.Name)
                {
                    items[i] = item;
                    return -1;
                }
            }
            items.Add(item);
            UpdatePanelHeight();
            SelectedIndex = items.Count - 1;
            EnsureVisible(SelectedIndex);
            return SelectedIndex;
        }

        public int RemoveAt(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                items.RemoveAt(index);
                UpdatePanelHeight();
                SelectedIndex = index - 1;
                panel1.Invalidate();
                return index;
            }
            return -1;
        }

        public int RemoveItem(ChatItem item)
        {
            int index = items.IndexOf(item);
            if (index != -1)
            {
                items.Remove(item);
                UpdatePanelHeight();
                SelectedIndex = index - 1;
                panel1.Invalidate();
            }
            return index;
        }

        public int ClearItems()
        {
            if (items.Count > 0)
            {
                items.Clear();
                UpdatePanelHeight();
                SelectedIndex = -1;
                panel1.Invalidate();
            }
            return 0;
        }

        public override void Refresh()
        {
            if (items.Count > 0)
            {
                UpdatePanelHeight();
                panel1.Invalidate();
            }
        }

        public bool ContainsItem(ChatItem item) => items.Contains(item);

        public string GetUniqueName(string baseName)
        {
            int count = 1;
            string agentName = $"{baseName} ({count})";
            while (items.Any(item => item.Name == agentName))
            {
                count++;
                agentName = $"{baseName} ({count})";
            }
            return agentName;
        }

        public void SelectItem(int index)
        {
            if (index >= 0 && index < items.Count && SelectedIndex != index)
            {
                SelectedIndex = index;
            }
        }

        public void SelectItem(ChatItem item)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (item.Name == items[i].Name && SelectedIndex != i)
                {
                    SelectedIndex = i;
                    break;
                }
            }
        }

        public void EnsureVisible(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                int itemTop = index * LABEL_HEIGHT;
                panelContainer.AutoScrollPosition = new Point(0, itemTop);
            }
        }

        public void SetText(string text)
        {
            if (text != GetSelectedText())
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (text == items[i].Name)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        public void SetMessages(int index, List<Message> messages)
        {
            if (index >= 0 && index < items.Count)
            {
                items[index].Messages = messages;
                panel1.Invalidate();
            }
        }

        public void SetSelectedMessages(List<Message> messages)
        {
            if (SelectedIndex >= 0 && SelectedIndex < items.Count)
            {
                items[SelectedIndex].Messages = messages;
                panel1.Invalidate();
            }
        }

        internal void SetSelectedItem(ChatItem? selectedItem)
        {
            if (SelectedIndex >= 0 && SelectedIndex < items.Count)
            {
                items[SelectedIndex] = selectedItem ?? new ChatItem();
                panel1.Invalidate();
            }
        }

        public string GetText(int index) => (index >= 0 && index < items.Count) ? items[index].Name : string.Empty;

        public string GetSelectedText() => (SelectedIndex >= 0 && SelectedIndex < items.Count) ? items[SelectedIndex].Name : string.Empty;

        public ChatItem? GetSelectedItem() => (SelectedIndex >= 0 && SelectedIndex < items.Count) ? items[SelectedIndex] : null;

        public List<ChatItem> GetAllItems() => [.. items];

        public int ItemCount() => items.Count;
    }
}