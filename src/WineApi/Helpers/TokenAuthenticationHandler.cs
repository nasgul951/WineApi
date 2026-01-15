using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using WineApi.Constants;
using WineApi.Service;

namespace WineApi.Helpers;

public class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAuthService _authService;

    public TokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAuthService authService)
        : base(options, logger, encoder)
    {
        _authService = authService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authHeader.ToString().Replace("Bearer ", string.Empty).Trim();
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.NoResult();
        }

        var user = await _authService.GetUserByToken(token);
        if (user == null)
        {
            return AuthenticateResult.Fail("Invalid token.");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        // Add User Role Claims
        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, UserRole.Admin));
        }

        var identity = new ClaimsIdentity(claims, nameof(TokenAuthenticationHandler));
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}