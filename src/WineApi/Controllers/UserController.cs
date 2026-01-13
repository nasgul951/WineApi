using Microsoft.AspNetCore.Mvc;
using WineApi.Attributes;
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
    [UsePaging]
    public async Task<IActionResult> Query(UserRequest req)
    {
        var query = _userService.GetByFilter(req);
        return Ok(query);
    }

}
