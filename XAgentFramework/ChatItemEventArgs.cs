namespace XAgentFramework
{
    public class ChatItemEventArgs(ChatItem? oldItem, ChatItem? newItem) : EventArgs
    {
        public ChatItem? OldItem { get; } = oldItem;

        public ChatItem? NewItem { get; } = newItem;
    }

}
