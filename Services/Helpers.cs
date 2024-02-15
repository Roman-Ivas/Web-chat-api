using System.Security.Claims;

namespace LAST.Services
{
    public class Helpers
    {
        public static int? GetUserIdFromClaims(ClaimsPrincipal user)
        {
            if (user == null)
                return null;

            var idClaim = user.FindFirst(AuthOptions.ClaimTypes.Id);
            if (idClaim == null)
                return null;

            var parseSucceded = int.TryParse(idClaim.Value, out int userId);
            if (!parseSucceded)
                throw new ArgumentException("User is unauthorized or doesn't have required claims!");

            return userId;
        }

        public static string GetUserNameFromClaims(ClaimsPrincipal user)
        {
            if (user == null)
                return null;

            var nameClaim = user.FindFirst(AuthOptions.ClaimTypes.Username);
            if (nameClaim == null)
                return null;

            return nameClaim.Value;
        }

        public static IEnumerable<string> ParseConversationName(string conversationName)
        {
            var nameStripFirstChar = conversationName.Remove(0, 1);
            var tokens = nameStripFirstChar.Split("<=>");
            return tokens;
        }
    }
}
