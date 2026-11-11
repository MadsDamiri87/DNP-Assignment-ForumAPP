using ApiContracts;
using Entities;
using Microsoft.EntityFrameworkCore;
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

    public async Task<UserDto?> ValidateUserAsync(AuthDto request)
    {
        User? user = await userRepository.GetMany()
            .FirstOrDefaultAsync(candidate =>
                candidate.UserName.ToLower() == request.UserName.Trim().ToLower());

        if (user == null || user.Password != request.Password)
        {
            return null;
        }
        return ToDto(user);
    }
    
    private static UserDto ToDto(User user)=>
        new(user.Id, user.UserName, user.Email, user.Role, user.CreatedAt);
}
