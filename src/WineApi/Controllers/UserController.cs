using Microsoft.AspNetCore.Mvc;
using WineApi.Model.User;
using WineApi.Service;

namespace WineApi.Controllers;

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
    public async Task<IActionResult> Query([FromQuery] PagedRequest<UserRequest, UserDto> req)
    {
        var q = _userService.GetByFilter(req.FilterObject);
        var response = await req.BuildResponseAsync(q);
        return Ok(response);
    }

}
