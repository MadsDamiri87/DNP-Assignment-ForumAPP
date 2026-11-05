using System.Security.Claims;

namespace WebAPI.Auth;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        Claim? claim = principal.FindFirst("Id");
        
        return claim is not null && int.TryParse(claim.Value, out int id) ? id : null;
        
    }
}
