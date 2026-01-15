using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineApi.Attributes;
using WineApi.Constants;
using WineApi.Filters;
using WineApi.Model.User;
using WineApi.Service;

namespace WineApi.Controllers;

[Authorize(Roles = UserRole.Admin)]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService; 
    }

    [HttpGet("query")]
    [UsePaging]
    public IQueryable<UserDto> Query([FromQuery] UserRequest req) => _userService.GetByFilter(req);
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUser(int id){
        var user = await _userService.GetById(id);
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(AddUpdateUser req) {
        var user = await _userService.AddUser(req);
        return Accepted(user);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, AddUpdateUser req) {
        var user = await _userService.UpdateUser(id, req);
        return Ok(user);
    }
}
