using LAST.Models.Api;
using LAST.Models.IdentityModels;

namespace LAST.Services
{
    public interface IAuthService
    {
        AuthResponse Authenticate(SignInCredentials credentials);
        void DeleteExpiredTokens();
        void Logout(string refreshToken);
        AppUser Register(RegistrationCredentials credentials);
        AuthResponse RenewToken(string refreshToken);
    }
}
