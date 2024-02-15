using LAST.Models.IdentityModels;

namespace LAST.Models.Api
{
    public class UserConversationActivity
    {
        public int UserId { get; set; }
        public int ConversationId { get; set; }
        public DateTime? LastConnect { get; set; }
        public DateTime? LastDisconnect { get; set; }

        public virtual AppUser User { get; set; }
        public virtual Conversation Conversation { get; set; }

        public UserConversationActivity(int userId, int conversationId)
        {
            UserId = userId;
            ConversationId = conversationId;
            LastConnect = null;
            LastDisconnect = null;
        }

        public UserConversationActivity(AppUser user, Conversation conversation)
        {
            UserId = user.Id;
            ConversationId = conversation.Id;
            LastConnect = null;
            LastDisconnect = null;
        }
    }
}
