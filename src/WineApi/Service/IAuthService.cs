using WineApi.Data;
using WineApi.Model.Auth;

namespace WineApi.Service;

public interface IAuthService
{
    Task<AuthResponse?> Authenticate(string username, string password);
    Task<User?> GetUserByToken(string token);
    Task ClearUserToken(int userId);
    Task<UserDto?> AddUser(AddUpdateUser newUser);
    Task<UserDto?> UpdateUser(int userId, AddUpdateUser newUser);
}
