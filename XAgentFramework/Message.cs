namespace XAgentFramework
{
    public class Message
    {
        public string Sender { get; set; } = string.Empty;

        public string Recipient { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime Time { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public List<string> ImagePaths { get; set; } = [];

        public Message()
        {
        }

        public Message(int sender, int recipient, string content, DateTime time, bool isRead)
        {
            Sender = sender.ToString();
            Recipient = recipient.ToString();
            Content = content;
            Time = time;
            IsRead = isRead;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Message other)
            {
                return Sender == other.Sender
                    && Recipient == other.Recipient
                    && Content == other.Content
                    && Math.Abs((Time - other.Time).TotalSeconds) < 1;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Sender, Recipient, Content, Time.Year, Time.DayOfYear, Time.Hour, Time.Minute, Time.Second);
        }
    }
}