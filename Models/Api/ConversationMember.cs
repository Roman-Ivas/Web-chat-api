using LAST.Models.IdentityModels;

namespace LAST.Models.Api
{
    public class ConversationMember
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }

        public ConversationMember()
        {
        }

        public ConversationMember(int id, string username, string email, bool isActive)
        {
            Id = id;
            Username = username;
            Email = email;
            IsActive = isActive;
        }

        public ConversationMember(AppUser user, bool isActive)
        {
            Id = user.Id;
            Username = user.UserName;
            Email = user.Email;
            IsActive = isActive;
        }
    }
}
