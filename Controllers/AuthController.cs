using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineApi.Service;

namespace WineApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _logger = logger;
        _authService = authService;
    }

    [HttpPost]
    public IActionResult Login(AuthRequest request)
    {
        try
        {
            var auth = _authService.Authenticate(request.UserName, request.Password).Result;

            if (auth == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(auth);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during authentication.");
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [Authorize(AuthenticationSchemes = "Token")]
    [HttpGet("userinfo")]
    public IActionResult GetUserInfo()
    {
        var principal = HttpContext.User;
        return Ok(new
        {
            UserId = principal.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = principal.FindFirstValue(ClaimTypes.Name)
        });
    }
}
