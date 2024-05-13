using Microsoft.AspNetCore.Mvc;

namespace WineApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    // [HttpPost]
    // public IEnumerable<WeatherForecast> Index()
    // {
    //     ret
    // }
}
