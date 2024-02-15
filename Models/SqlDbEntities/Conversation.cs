using LAST.Models.IdentityModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace LAST.Models.Api
{
    public class Conversation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Message> Messages { get; set; } = new List<Message>();
        public virtual List<AppUser> Participants { get; set; } = new List<AppUser>();
        public bool IsGroupChat { get; set; }

        public int? CreatorUserId { get; set; }
        [ForeignKey(nameof(CreatorUserId))]
        public virtual AppUser CreatedBy { get; set; }

        public static Conversation CreateGroupConversation(AppUser creator, string groupName)
        {
            var conv = new Conversation()
            {
                Name = groupName,
                IsGroupChat = true,
                CreatedBy = creator
            };
            conv.Participants.Add(creator);
            return conv;
        }

        public static Conversation CreatePrivateConversation(AppUser user1, AppUser user2)
        {
            var groupName = BuildPrivateConversationName(user1, user2);
            var conv = new Conversation()
            {
                Name = groupName,
                IsGroupChat = false
            };
            conv.Participants.Add(user1);
            conv.Participants.Add(user2);
            return conv;
        }

        public static string BuildPrivateConversationName(AppUser user1, AppUser user2)
        {
            var emails = new List<AppUser>() {
                user1, user2
            }
            .OrderBy(u => u.Email.ToLower())
            .Select(u => u.Email);
            var groupName = "@" + String.Join("<=>", emails);
            return groupName;
        }
    }
}
