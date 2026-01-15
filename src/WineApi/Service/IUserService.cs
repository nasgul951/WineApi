using WineApi.Model.User;

namespace WineApi.Service;

public interface IUserService
{
    Task<UserDto> GetById(int userId);
    IQueryable<UserDto> GetByFilter(UserRequest filter);
    Task<UserDto?> AddUser(AddUpdateUser newUser);
    Task<UserDto?> UpdateUser(int userId, AddUpdateUser newUser);
}
