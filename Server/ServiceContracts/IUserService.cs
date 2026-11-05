using ApiContracts;

namespace ServiceContracts;

public interface IUserService : ICreatableService<UserDto, CreateUserDto, UpdateUserDto>
{
    IEnumerable<UserDto> GetMany(
        string? userNameContains = null);
}
