using ApiContracts;

namespace ServiceContracts;

public interface IUserService
{
    Task<UserDto> CreateAsync(CreateUserDto request);
    Task<UserDto> UpdateAsync(int id, UpdateUserDto request);
    Task DeleteAsync(int id);
    Task<UserDto> GetSingleAsync(int id);
    IEnumerable<UserDto> GetMany(string? userNameContains = null);
}
