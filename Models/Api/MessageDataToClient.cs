namespace LAST.Models.Api
{
    public class MessageDataToClient
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public int ConversationId { get; set; }
        public DateTime SentAt { get; set; }

        public MessageDataToClient(Message message)
        {
            Id = message.Id;
            Text = message.Text;
            // message might not have related entities (eg User) loaded
            SenderId = message.User?.Id ?? 0;
            SenderName = message.User?.UserName;
            ConversationId = message.ConversationId;
            SentAt = message.SentAt;
        }
    }
}
