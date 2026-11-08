using System.Security.Claims;
using Blazor_Application.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blazor_Application.Auth;

public class CustomAuthProvider : AuthenticationStateProvider
{
    private readonly IAuthApiService _authApiService;

    public CustomAuthProvider(IAuthApiService authApiService)
    {
        this._authApiService = authApiService;
        authApiService.OnAuthStateChanged += AuthStateChanged;
    }


    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsPrincipal principal = await _authApiService.GetAuthAsync();
        return new AuthenticationState(principal);
    }

    private void AuthStateChanged(ClaimsPrincipal principal)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }
}