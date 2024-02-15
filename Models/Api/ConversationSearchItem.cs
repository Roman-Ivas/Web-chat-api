using LAST.Models.IdentityModels;

namespace LAST.Models.Api
{
    public class ConversationSearchItem
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public bool IsUser { get; set; }

        public ConversationSearchItem()
        {
        }

        public ConversationSearchItem(Conversation conversation)
        {
            Name = conversation.Name;
            Id = conversation.Id;
            IsUser = false;
        }

        public ConversationSearchItem(AppUser user)
        {
            Name = user.UserName;
            Id = user.Id;
            IsUser = true;
        }
    }
}
