using ApiContracts;

namespace Blazor_Application.Services;

public class HttpUserService : HttpServiceBase<UserDto, CreateUserDto, UpdateUserDto>, IUserService
{
    public HttpUserService(HttpClient client) : base(client, "users")
    {
    }

    public Task<List<UserDto>> GetUsersAsync(string? userNameContains = null) =>
        GetManyAsync(("userNameContains", userNameContains));
}
