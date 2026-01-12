using WineApi.Data;

namespace WineApi.Service;

public interface IAuthService
{
    Task<AuthResponse?> Authenticate(string username, string password);
    Task<User?> GetUserByToken(string token);
    Task ClearUserToken(int userId);
}
