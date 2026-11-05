using System.Security.Claims;

namespace Blazor_Application.Services;

public interface IAuthService
{
    Task LoginAsync(string userName, string password);
    Task LogoutAsync();
    Task<ClaimsPrincipal> GetAuthAsync();
    string? Jwt { get; }
    event Action<ClaimsPrincipal>? OnAuthStateChanged;
}