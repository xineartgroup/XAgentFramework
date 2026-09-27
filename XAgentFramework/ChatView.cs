using System.ComponentModel;

namespace XAgentFramework
{
    public partial class ChatView : UserControl
    {
        private readonly Dictionary<string, Bitmap> imageDictionary = [];
        private readonly Panel messagesContainer = new();
        private List<Message> _currentMessages = [];

        public event EventHandler? SelectionChangeEvent;

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
                List<Bitmap> attachmentBmps = GetAttachmentBitmaps(message);
                var messageItem = new ChatMessageItem(message, attachmentBmps);

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
        }

        private static List<Bitmap> GetAttachmentBitmaps(Message message)
        {
            List<Bitmap> bitmaps = [];
            foreach (string imagePath in message.ImagePaths)
            {
                Bitmap bmp = new(imagePath);
                bitmaps.Add(bmp);
            }
            return bitmaps;
        }
    }
}