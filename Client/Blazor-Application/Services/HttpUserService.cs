using ApiContracts;

namespace Blazor_Application.Services;

public class HttpUserService
{
    private readonly HttpClient client;

    public HttpUserService(HttpClient client)
    {
        this.client = client;
    }

    public Task<UserDto> AddUserAsync(CreateUserDto request)
    {
        
    }
    
    public Task UpdateUserAsync(int id, UpdateUserDto  request)
    {
        
    }
}