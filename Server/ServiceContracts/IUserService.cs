using ApiContracts;

namespace ServiceContracts;

public interface IUserService : ICreatableService<UserDto, CreateUserDto, UpdateUserDto>
{
    Task<IEnumerable<UserDto>> GetManyAsync(
        string? userNameContains = null);
}
