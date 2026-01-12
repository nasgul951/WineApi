using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Exceptions;
using WineApi.Extensions;
using WineApi.Helpers;
using WineApi.Model.User;

namespace WineApi.Service
{
    public class UserService : IUserService
    {
        private readonly WineContext _db;
        public UserService(WineContext wineContext)
        {
            _db = wineContext;
        }

        public IQueryable<UserDto> GetByFilter(UserRequest filter)
        {
            var users = _db.Users
                .IfThenWhere(!string.IsNullOrEmpty(filter.Username), u => EF.Functions.Like(u.Username, $"%{filter.Username}%"))
                .ToUserDto();
            
            return users;
        }

        public async Task<UserDto> GetById(int userId)
        {
            var user = await _db.Users
                .Where(u => u.Id == userId)
                .ToUserDto()
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new NotFoundException("userId");
            }
            
            return user;
        }

        public async Task<UserDto?> AddUser(AddUpdateUser addUser)
        {
            // Ensure username was provided
            if (string.IsNullOrWhiteSpace(addUser.Username))
            {
                throw new InvalidRequestException("username");
            }

            // Ensure unique username
            var userExists = await _db.Users.AnyAsync(u => u.Username == addUser.Username);
            if (userExists)
            {
                throw new ConflictException("Username");
            }

            // Use a random password if not specified
            if (string.IsNullOrWhiteSpace(addUser.Password))
            {
                addUser.Password = CryptoHelper.GenerateSalt(16);
            }

            var salt = CryptoHelper.GenerateSalt(16);
            var hashedPassword = CryptoHelper.HashPassword(addUser.Password, salt);

            var user = new User
            {
                Username = addUser.Username,
                IsAdmin = addUser.IsAdmin ?? false,
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

            if (!string.IsNullOrWhiteSpace(updateUser.Username))
            {
                user.Username = updateUser.Username;
            }

            if (!string.IsNullOrWhiteSpace(updateUser.Password))
            {
                var hashedPassword = CryptoHelper.HashPassword(updateUser.Password, user.Salt);
                user.Password = hashedPassword;
            }

            if (updateUser.IsAdmin.HasValue)
            {
                user.IsAdmin = updateUser.IsAdmin.Value;
            }

            var userDto = await _db.Users
                .Where(u => u.Id == userId)
                .ToUserDto()
                .FirstOrDefaultAsync();
            return userDto;
        }

    }
}
