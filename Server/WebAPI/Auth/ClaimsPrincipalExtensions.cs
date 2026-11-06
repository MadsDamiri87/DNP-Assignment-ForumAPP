using System.Security.Claims;
using ServiceContracts;

namespace WebAPI.Auth;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        Claim? claim = principal.FindFirst("Id");
        
        return claim is not null && int.TryParse(claim.Value, out int id) ? id : null;
        
    }
    
    public static Caller? GetCaller(this ClaimsPrincipal principal)
    {
        int? userId = principal.GetUserId();

        if (userId is null)
        {
            return null;
        }

        return new Caller(userId.Value, principal.IsInRole("Admin"));
        
    }
}
