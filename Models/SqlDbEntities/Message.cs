using LAST.Models.IdentityModels;
using System.Text.Json.Serialization;

namespace LAST.Models.Api
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; }

        public int UserId { get; set; }
        public virtual AppUser User { get; set; }

        public int ConversationId { get; set; }
        [JsonIgnore]
        public virtual Conversation Conversation { get; set; }

        public Message(string text, int userId, int conversationId)
        {
            Text = text;
            UserId = userId;
            ConversationId = conversationId;
            SentAt = DateTime.UtcNow;
        }
    }
}
