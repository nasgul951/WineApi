using Microsoft.EntityFrameworkCore;
using WineApi.Data;

namespace WineApi.Service;

public class AuthService
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

        var hashedPassword = HashPassword(password, user.Salt);
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

    private async Task GenerateUserKey(User user)
    {
        var guid = Guid.NewGuid();
        user.Key = guid.ToString().Replace("-", "").ToLowerInvariant(); // Generate a new key
        user.KeyExpires = DateTime.UtcNow.AddDays(7); // Key valid for 7 days
        user.LastOn = DateTime.UtcNow.ToString("o"); // Update last login time
        await _db.SaveChangesAsync();
    }

    private string HashPassword(string password, string salt)
    {
        // Calculate the sha256 hash of the password with the salt
        var saltedPassword = salt + password;
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(saltedPassword));
 
        // Return the hash as Hexadecimal string
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}