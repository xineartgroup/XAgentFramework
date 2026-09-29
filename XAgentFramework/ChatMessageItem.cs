using System.Drawing.Drawing2D;
using System.Net.Mail;
using static System.Windows.Forms.DataFormats;

namespace XAgentFramework
{
    public partial class ChatMessageItem : Panel
    {
        private const int SPACINGX = 10;
        private const int SPACINGY = 5;
        private const int ATTACHMENT_MAX_HEIGHT = 200;
        private const int ATTACHMENT_GAP = 8;
        private const int BUTTON_SIZE = 24;
        private const int GAP = 4;

        private readonly Message _message;
        private readonly List<Attachment> _attachments = [];

        private readonly Button btnCopy = new();
        private readonly Button btnDelete = new();
        private readonly Button btnSave = new();

        public event EventHandler<Message>? CopyRequested;
        public event EventHandler<Message>? DeleteRequested;
        public event EventHandler<Message>? SaveRequested;

        private List<RenderedHyperlink> _renderedLinks = [];

        public ChatMessageItem(Message message, List<Attachment> attachments)
        {
            _message = message;
            if (attachments != null)
            {
                _attachments.AddRange(attachments);
            }

            DoubleBuffered = true;
            BackColor = Color.Transparent;

            btnCopy.Size = new Size(BUTTON_SIZE, BUTTON_SIZE);
            btnCopy.FlatStyle = FlatStyle.Flat;
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.Cursor = Cursors.Hand;
            btnCopy.Paint += DrawCopyIcon;
            btnCopy.Click += (s, e) => CopyRequested?.Invoke(this, _message);

            btnDelete.Size = new Size(BUTTON_SIZE, BUTTON_SIZE);
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Paint += DrawDeleteIcon;
            btnDelete.Click += (s, e) => DeleteRequested?.Invoke(this, _message);

            btnSave.Size = new Size(BUTTON_SIZE, BUTTON_SIZE);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Cursor = Cursors.Hand;
            btnSave.Paint += DrawSaveIcon;
            btnSave.Click += BtnSave_Click;

            Controls.Add(btnCopy);
            Controls.Add(btnDelete);
            Controls.Add(btnSave);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (SaveRequested != null)
            {
                SaveRequested.Invoke(this, _message);
                return;
            }

            using SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.FileName = $"Message_{_message.Time:yyyyMMdd_HHmmss}.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, _message.Content);
            }
        }

        private void DrawCopyIcon(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using Pen pen = new(Color.FromArgb(70, 70, 70), 1.5f);
            using Brush fillBrush = new SolidBrush(btnCopy.BackColor);

            g.DrawRectangle(pen, 6, 6, 9, 11);

            Rectangle frontRect = new(9, 9, 9, 11);
            g.FillRectangle(fillBrush, frontRect);
            g.DrawRectangle(pen, frontRect);
        }

        private void DrawDeleteIcon(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using Pen pen = new(Color.FromArgb(180, 50, 50), 1.5f);

            g.DrawRectangle(pen, 10, 5, 4, 2);
            g.DrawLine(pen, 6, 7, 18, 7);

            Point[] binBody =
            [
                new Point(7, 8),
                new Point(8, 18),
                new Point(16, 18),
                new Point(17, 8)
            ];

            g.DrawLines(pen, binBody);
        }

        private void DrawSaveIcon(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using Pen pen = new(Color.FromArgb(50, 100, 180), 1.5f);
            using Brush fillBrush = new SolidBrush(btnSave.BackColor);

            Rectangle bodyRect = new(6, 5, 12, 14);
            g.DrawRectangle(pen, bodyRect);

            g.DrawRectangle(pen, 9, 5, 6, 4);

            Rectangle sliderRect = new(8, 12, 8, 7);
            g.FillRectangle(fillBrush, sliderRect);
            g.DrawRectangle(pen, sliderRect);
        }

        public void CalculateLayout(int availableWidth)
        {
            using Graphics g = CreateGraphics();
            GetDimensions(g, availableWidth, out int bubbleWidth, out int bubbleHeight, out _, out int x);

            int buttonsX = x + bubbleWidth - (BUTTON_SIZE * 3) - (GAP * 2) - (SPACINGX * 2);
            int buttonsY = SPACINGY + bubbleHeight - BUTTON_SIZE - SPACINGY;

            btnCopy.Location = new Point(buttonsX, buttonsY);
            btnSave.Location = new Point(buttonsX + BUTTON_SIZE + GAP, buttonsY);
            btnDelete.Location = new Point(buttonsX + (BUTTON_SIZE * 2) + (GAP * 2), buttonsY);

            Color bubbleBg = _message.Sender == "Me" ? Color.LightGreen : Color.AliceBlue;
            btnCopy.BackColor = bubbleBg;
            btnSave.BackColor = bubbleBg;
            btnDelete.BackColor = bubbleBg;

            this.Size = new Size(availableWidth, bubbleHeight + (SPACINGY * 2));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            GetDimensions(g, Width, out int bubbleWidth, out int bubbleHeight, out int contentHeight, out int x);

            string sender = _message.Sender;
            string txtTime = $"{_message.Time:hh:mm tt}";

            using Font fontContent = new("Arial", 10);
            using Font fontSender = new("Arial", 8, FontStyle.Bold);
            using Font fontTime = new("Arial", 8);

            SizeF fontSenderSize = g.MeasureString(sender, fontSender);
            Brush brush = sender == "Me" ? Brushes.LightGreen : Brushes.AliceBlue;

            Point pointSender = new(SPACINGX + x, 2 * SPACINGY);
            Point pointTime = new((int)fontSenderSize.Width + (2 * SPACINGX) + x, 2 * SPACINGY);
            Rectangle rect = new(x, SPACINGY, bubbleWidth, bubbleHeight);
            int contentStartY = (3 * SPACINGY) + (int)fontSenderSize.Height;
            Rectangle rectContent = new(SPACINGX + x, contentStartY, bubbleWidth - (2 * SPACINGX), contentHeight);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = CreateRoundedRectanglePath(rect, 12))
            {
                g.FillPath(brush, path);
            }

            g.SmoothingMode = SmoothingMode.Default;

            g.DrawString(sender, fontSender, Brushes.Brown, pointSender);
            g.DrawString(txtTime, fontTime, Brushes.Gray, pointTime);

            if (_attachments.Count > 0)
            {
                int currentY = contentStartY;
                int maxAvailableWidth = rectContent.Width;

                foreach (Attachment att in _attachments)
                {
                    if (att.Image != null)
                    {
                        SizeF scaledSize = GetProportionalSize(att.Image.Size, maxAvailableWidth, ATTACHMENT_MAX_HEIGHT);
                        Rectangle imgRect = new(SPACINGX + x, currentY, (int)scaledSize.Width, (int)scaledSize.Height);

                        g.DrawImage(att.Image, imgRect);
                        currentY += (int)scaledSize.Height + ATTACHMENT_GAP;
                    }
                    else
                    {
                        RectangleF textRect = new(SPACINGX + x, currentY, maxAvailableWidth, 24);

                        g.DrawString(att.FileName, fontContent, Brushes.Black, textRect);
                        currentY += 24 + ATTACHMENT_GAP;
                    }
                }

                if (!string.IsNullOrWhiteSpace(_message.Content) && !_message.Content.StartsWith("[Attachment:"))
                {
                    Rectangle textRect = new(SPACINGX + x, currentY, maxAvailableWidth, contentHeight - (currentY - contentStartY));
                    MiniMarkdownRenderer.DrawMarkdown(g, _message.Content, fontContent, Color.Black, textRect);
                }
            }
            else
            {
                MiniMarkdownRenderer.DrawMarkdown(g, _message.Content, fontContent, Color.Black, rectContent);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            string? clickedUrl = MiniMarkdownRenderer.GetClickedUrl(_renderedLinks, e.Location);
            if (!string.IsNullOrEmpty(clickedUrl))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = clickedUrl,
                    UseShellExecute = true
                });
            }
        }

        private void GetDimensions(Graphics g, int totalAvailableWidth, out int bubbleWidth, out int bubbleHeight, out int contentHeight, out int x)
        {
            string sender = _message.Sender;
            string txtTime = $"{_message.Time:hh:mm tt}";

            using Font fontContent = new("Arial", 10);
            using Font fontSender = new("Arial", 8, FontStyle.Bold);
            using Font fontTime = new("Arial", 8);

            int maxContentWidth = totalAvailableWidth - (6 * SPACINGX);
            Size fontContentSize = MiniMarkdownRenderer.MeasureMarkdown(g, _message.Content, fontContent, maxContentWidth);
            SizeF fontSenderSize = g.MeasureString(sender, fontSender);
            SizeF fontTimeSize = g.MeasureString(txtTime, fontTime);

            int attachmentsHeight = 0;
            int maxAttachmentWidth = 0;

            if (_attachments.Count > 0)
            {
                foreach (Attachment att in _attachments)
                {
                    if (att.Image != null)
                    {
                        SizeF scaledSize = GetProportionalSize(att.Image.Size, maxContentWidth, ATTACHMENT_MAX_HEIGHT);
                        attachmentsHeight += (int)scaledSize.Height + ATTACHMENT_GAP;

                        if ((int)scaledSize.Width > maxAttachmentWidth)
                        {
                            maxAttachmentWidth = (int)scaledSize.Width;
                        }
                    }
                    else
                    {
                        // Measure non-image text representation directly
                        SizeF textAttachmentSize = g.MeasureString(att.FileName, fontContent, maxContentWidth);

                        // Add padding for file chip background (e.g., 20px padding)
                        int chipWidth = Math.Min((int)textAttachmentSize.Width + 20, maxContentWidth);
                        int chipHeight = 32; // Fixed height for non-image attachment chips

                        attachmentsHeight += chipHeight + ATTACHMENT_GAP;

                        if (chipWidth > maxAttachmentWidth)
                        {
                            maxAttachmentWidth = chipWidth;
                        }
                    }
                }

                int textExtraHeight = (!string.IsNullOrWhiteSpace(_message.Content) && !_message.Content.StartsWith("[Attachment:"))
                    ? (int)fontContentSize.Height + SPACINGY
                    : 0;

                contentHeight = attachmentsHeight + textExtraHeight;
            }
            else
            {
                contentHeight = (int)fontContentSize.Height;
            }

            int headerWidth = (int)(fontSenderSize.Width + fontTimeSize.Width + (3 * SPACINGX));
            int actionsWidth = (BUTTON_SIZE * 3) + (GAP * 2) + (4 * SPACINGX);

            int bodyWidth = _attachments.Count > 0
                ? Math.Max(maxAttachmentWidth, (int)fontContentSize.Width) + (4 * SPACINGX)
                : (int)fontContentSize.Width + (4 * SPACINGX);

            bubbleWidth = Math.Max(headerWidth, bodyWidth);
            bubbleWidth = Math.Max(bubbleWidth, actionsWidth);

            int headerHeight = (int)Math.Max(fontTimeSize.Height, fontSenderSize.Height);
            bubbleHeight = headerHeight + contentHeight + BUTTON_SIZE + (SPACINGY * 5);

            int totalWidth = totalAvailableWidth - SPACINGX;
            x = sender == "Me" ? totalWidth - bubbleWidth - SPACINGX : SPACINGX;
        }

        private static SizeF GetProportionalSize(Size originalSize, int maxWidth, int maxHeight)
        {
            float ratioX = (float)maxWidth / originalSize.Width;
            float ratioY = (float)maxHeight / originalSize.Height;
            float ratio = Math.Min(ratioX, ratioY);

            // Keep original size if it's smaller than maximum bounds
            if (ratio > 1.0f) ratio = 1.0f;

            return new SizeF(originalSize.Width * ratio, originalSize.Height * ratio);
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            GraphicsPath path = new();
            int diameter = cornerRadius * 2;

            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = rect.Height;

            Rectangle arc = new(rect.X, rect.Y, diameter, diameter);

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.X;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}