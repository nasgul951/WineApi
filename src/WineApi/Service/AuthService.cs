using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Exceptions;
using WineApi.Extensions;
using WineApi.Helpers;
using WineApi.Model.Auth;

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
    
    public async Task<UserDto?> AddUser(AddUpdateUser addUser)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Username  == addUser.Username);
        if (userExists) {
            throw new ConflictException("Username");
        }

        var salt = CryptoHelper.GenerateSalt(16);
        var hashedPassword = CryptoHelper.HashPassword(addUser.Password, salt);

        var user = new User
        {
            Username = addUser.Username,
            IsAdmin = addUser.IsAdmin,
            Salt = salt,
            Password = hashedPassword
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var userDto = await _db.Users
            .Where(u => u.Username == addUser.Username)
            .ToUserDto()
            .FirstOrDefaultAsync();
        return userDto;
    }

    public async Task<UserDto?> UpdateUser(int userId, AddUpdateUser updateUser)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new NotFoundException("userId");
        }

        var usernameExists = await _db.Users.AnyAsync(u => u.Username == updateUser.Username & u.Id != userId);
        if (usernameExists)
        {
            throw new ConflictException("Username");
        }

        var hashedPassword = CryptoHelper.HashPassword(updateUser.Password, user.Salt);
        user.Username = updateUser.Username;
        user.Password = hashedPassword;
        user.IsAdmin = updateUser.IsAdmin;

        var userDto = await _db.Users
            .Where(u => u.Id == userId)
            .ToUserDto()
            .FirstOrDefaultAsync();
        return userDto;
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