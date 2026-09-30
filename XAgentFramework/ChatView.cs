namespace XAgentFramework
{
    public partial class ChatView : UserControl
    {
        private readonly Dictionary<string, Bitmap> imageDictionary = [];
        private readonly Panel messagesContainer = new();
        private List<Message> _currentMessages = [];

        public event EventHandler<ChatMessageEventArgs>? DeleteRequested = null;

        public ChatView()
        {
            InitializeComponent();

            panelContainer.AutoScroll = true;

            panel.Dock = DockStyle.Top;
            panel.AutoSize = true;
            panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            messagesContainer.AutoScroll = false;
            messagesContainer.Dock = DockStyle.Top;
            panel.Controls.Add(messagesContainer);

            SizeChanged += ChatView_SizeChanged;
        }

        private void ChatView_SizeChanged(object? sender, EventArgs e)
        {
            if (_currentMessages == null) return;
            RenderMessages(_currentMessages);
        }

        public void RenderMessages(List<Message> messages)
        {
            _currentMessages = messages;

            if (Width <= 0) return;

            panelContainer.SuspendLayout();
            messagesContainer.Controls.Clear();

            int currentY = 0;

            int containerWidth = panelContainer.ClientSize.Width;

            foreach (var message in _currentMessages)
            {
                List<Attachment> attachments = GetAttachments(message);
                var messageItem = new ChatMessageItem(message, attachments);

                messageItem.CopyRequested += MessageItem_CopyRequested;
                messageItem.DeleteRequested += MessageItem_DeleteRequested;

                messageItem.CalculateLayout(containerWidth);
                messageItem.Location = new Point(0, currentY);

                messagesContainer.Controls.Add(messageItem);
                currentY += messageItem.Height;
            }

            messagesContainer.Size = new Size(containerWidth, currentY);
            panelContainer.ResumeLayout();

            panelContainer.ScrollControlIntoView(messagesContainer);
            if (panelContainer.VerticalScroll.Visible)
            {
                panelContainer.VerticalScroll.Value = panelContainer.VerticalScroll.Maximum;
            }
        }

        private void MessageItem_CopyRequested(object? sender, Message message)
        {
            if (!string.IsNullOrEmpty(message.Content))
            {
                Clipboard.SetText(message.Content);
            }
        }

        private void MessageItem_DeleteRequested(object? sender, Message message)
        {
            if (_currentMessages == null) return;
            _currentMessages.Remove(message);
            RenderMessages(_currentMessages);
            DeleteRequested?.Invoke(this, new ChatMessageEventArgs(message, _currentMessages));
        }

        private static List<Attachment> GetAttachments(Message message)
        {
            List<Attachment> attachments = [];
            HashSet<string> validExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif"];

            foreach (string filePath in message.FilePaths)
            {
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                if (validExtensions.Contains(extension))
                {
                    try
                    {
                        attachments.Add(new Attachment { Image = new Bitmap(filePath) });
                    }
                    catch
                    {
                        // Fall back to filename text on load failure
                    }
                }
                else
                {
                    attachments.Add(new Attachment { FileName = Path.GetFileName(filePath) });
                }
            }

            return attachments;
        }
    }
}