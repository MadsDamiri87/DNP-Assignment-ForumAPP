using ApiContracts;

namespace ServiceContracts;

public interface IAuthService
{
    Task<UserDto?> ValidateUserAsync(AuthDto request);
}
