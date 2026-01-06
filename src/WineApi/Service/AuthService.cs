using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Exceptions;
using WineApi.Helpers;

namespace WineApi.Service;

public class AuthService : IAuthService
{
    private readonly WineContext _db;

    public AuthService(WineContext wineContext)
    {
        _db = wineContext;
    }

    public async Task<AuthResponse?> Authenticate(string username, string password)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            return null; // User not found
        }

        var hashedPassword = CryptoHelper.HashPassword(password, user.Salt);
        if (hashedPassword != user.Password)
        {
            return null; // Password does not match
        }

        await GenerateUserKey(user);

        return new AuthResponse
        {
            Token = user.Key!,
            Expires = user.KeyExpires!.Value
        };
    }
    
    public async Task<User?> GetUserByToken(string token)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Key == token && u.KeyExpires > DateTime.UtcNow);

        return user;
    }

    public async Task ClearUserToken(int userId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new InvalidRequestException(nameof(userId));
        }

        user.Key = null;
        user.KeyExpires = null;
        await _db.SaveChangesAsync();
    }
    
    private async Task GenerateUserKey(User user)
    {
        var guid = Guid.NewGuid();
        user.Key = guid.ToString().Replace("-", "").ToLowerInvariant(); // Generate a new key
        user.KeyExpires = DateTime.UtcNow.AddDays(7); // Key valid for 7 days
        user.LastOn = DateTime.UtcNow.ToString("o"); // Update last login time
        await _db.SaveChangesAsync();
    }
}