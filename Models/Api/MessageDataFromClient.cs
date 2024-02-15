namespace LAST.Models.Api
{
    public class MessageDataFromClient
    {
        public int? ReceiverConversationId { get; set; }
        public int? ReceiverUserId { get; set; }
        public string MessageText { get; set; }
    }
}
