namespace LAST.Models.Api
{
    public class ChatRoomData
    {
        public IEnumerable<MessageDataToClient> LatestMessages { get; set; }
        public IEnumerable<ConversationMember> Members { get; set; }
    }
}
