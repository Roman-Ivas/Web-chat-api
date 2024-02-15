using LAST.Models.IdentityModels;
using System.ComponentModel.DataAnnotations;

namespace LAST.Models.Api
{
    public class RefreshToken
    {
        [Key]
        public string Token { get; set; }
        public DateTime TimeCreated { get; set; }

        public int UserId { get; set; }
        public virtual AppUser User { get; set; }
    }
}
