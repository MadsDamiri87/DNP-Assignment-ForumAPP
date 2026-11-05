using ApiContracts;
using Entities;
using RepositoryContracts;
using ServiceContracts;

namespace Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository userRepository;

    public AuthService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public Task<UserDto?> ValidateUserAsync(AuthDto request)
    {
        User? user = userRepository.GetMany()
            .FirstOrDefault(candidate =>
                candidate.UserName.ToLower() == request.UserName.Trim().ToLower());

        if (user == null || user.Password != request.Password)
        {
            return Task.FromResult<UserDto?>(null);
        }
        return Task.FromResult<UserDto?>(ToDto(user));
    }
    
    private static UserDto ToDto(User user)=>
        new(user.Id, user.UserName, user.Email, user.Role, user.CreatedAt);
}