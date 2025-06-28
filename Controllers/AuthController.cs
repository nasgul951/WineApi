using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using WineApi.Exceptions;
using WineApi.Extensions;
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

    [Authorize(AuthenticationSchemes = "Token")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var principal = HttpContext.User;
            var userId = principal.FindFirstValueAsInt(ClaimTypes.NameIdentifier);
            await _authService.ClearUserToken(userId);

            return Ok("Logged out");
        }
        catch (InvalidRequestException ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(400, "Bad request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during logout.");
            return StatusCode(500, "Internal server error");
        }
    }
}
