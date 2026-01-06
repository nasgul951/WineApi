using System.Security.Claims;

namespace WineApi.Extensions;

public static class ClaimsPrincipalEx
{
  public static int FindFirstValueAsInt(this ClaimsPrincipal principal, string claimType)
  {
    var claim = principal.FindFirst(claimType);
    if (claim == null || !int.TryParse(claim.Value, out var value))
    {
      throw new InvalidOperationException($"Claim '{claimType}' not found or is not an integer.");
    }
    return value;
  }
}