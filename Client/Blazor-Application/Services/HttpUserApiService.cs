using ApiContracts;

namespace Blazor_Application.Services;

public class HttpUserApiService : HttpServiceBase<UserDto, CreateUserDto, UpdateUserDto>, IUserApiService
{
    public HttpUserApiService(HttpClient client) : base(client, "users")
    {
    }

    public Task<List<UserDto>> GetUsersAsync(string? userNameContains = null) =>
        GetManyAsync(("userNameContains", userNameContains));
}
