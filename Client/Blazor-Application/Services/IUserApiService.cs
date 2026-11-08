using ApiContracts;

namespace Blazor_Application.Services;

public interface IUserApiService : IHttpService<UserDto, CreateUserDto, UpdateUserDto>
{
    Task<List<UserDto>> GetUsersAsync(string? userNameContains = null);
}
