using ApiContracts;

namespace ServiceContracts;

public interface IUserService : IService<UserDto, CreateUserDto, UpdateUserDto>
{
    IEnumerable<UserDto> GetMany(
        string? userNameContains = null);
}
