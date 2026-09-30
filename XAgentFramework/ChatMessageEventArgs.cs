namespace XAgentFramework
{
    public class ChatMessageEventArgs(Message? message, List<Message> currentMessages) : EventArgs
    {
        public Message? DeletedMessage { get; } = message;

        public List<Message> CurrentMessages { get; set; } = currentMessages;
    }

}
