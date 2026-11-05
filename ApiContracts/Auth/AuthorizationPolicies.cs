using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiContracts.Auth;

public static class AuthorizationPolicies
{
    public static void AddPolicies(IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
            options.AddPolicy("MustBeAdmin", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("Role", "Admin")));
        
    }
}