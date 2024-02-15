namespace LAST.Models.Api
{
    public class ConversationListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public MessageDataToClient LastMessage { get; set; }
        public int NewMessages { get; set; }
        /*User id of the other private conversation member*/
        public int? OtherUserId { get; set; }

        public ConversationListItem()
        {
        }

        public ConversationListItem(int id, string name, Message lastMessage, int newMessages, int? otherUserId)
        {
            Id = id;
            Name = name;
            LastMessage = lastMessage != null ? new MessageDataToClient(lastMessage) : null;
            NewMessages = newMessages;
            OtherUserId = otherUserId;
        }
    }
}
