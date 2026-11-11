using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace WebAPI.Auth;

public class UserTokenEvents : JwtBearerEvents
{
    private readonly IUserRepository userRepository;

    public UserTokenEvents(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        int? userId = context.Principal?.GetUserId();

        if (userId is null)
        {
            context.Fail("Invalid user.");
            return;
        }

        bool active = await userRepository.GetMany()
            .AnyAsync(user => user.Id == userId.Value && !user.IsDeleted);

        if (!active)
        {
            context.Fail("User no longer exists.");
        }
    }
}
