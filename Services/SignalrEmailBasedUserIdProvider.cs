using Microsoft.AspNetCore.SignalR;

namespace LAST.Services
{
    public class SignalrEmailBasedUserIdProvider: IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(AuthOptions.ClaimTypes.Email)?.Value;
        }
    }
}
