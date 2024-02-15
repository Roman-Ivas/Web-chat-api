using LAST.Models.Api;
using Microsoft.AspNetCore.Identity;
//using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LAST.Models.IdentityModels
{
    public class AppUser: IdentityUser<int>
    {
        public string Role { get; set; } = "User";
        [JsonIgnore]
        public string Password { get; set; }
        public DateTime? LastConnect { get; set; }
        public DateTime? LastDisconnect { get; set; }

        [JsonIgnore]
        public virtual List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        [JsonIgnore]
        public virtual List<Conversation> Conversations { get; set; } = new List<Conversation>();
        [JsonIgnore]
        [InverseProperty(nameof(Conversation.CreatedBy))]
        public virtual List<Conversation> CreatedConversations { get; set; } = new List<Conversation>();
        [JsonIgnore]
        public virtual List<Message> Messages { get; set; } = new List<Message>();

        public AppUser()
        {
        }

        public AppUser(RegistrationCredentials credentials)
        {
            UserName = credentials.Username;
            Email = credentials.Email;
            Password = credentials.Password;
        }
    }
}
