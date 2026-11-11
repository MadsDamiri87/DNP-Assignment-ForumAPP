using System.Security.Claims;
using System.Text;
using Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RepositoryContracts;
using ServiceContracts;
using Services;
using Tests.UnitTests.Fakes;
using WebAPI.Auth;

namespace Tests.IntegrationTests;

public class UserTokenTests
{
    private static readonly SymmetricSecurityKey Key =
        new(Encoding.UTF8.GetBytes("test-key-for-user-deletion-at-least-32-bytes"));

    [Fact]
    public async Task ShouldRejectTheExistingToken_WhenTheAccountIsDeleted()
    {
        FakeUserRepository users = CreateUsers();
        await using ServiceProvider services = CreateServices(users);
        string token = CreateToken("1");

        Assert.True((await AuthenticateAsync(services, token)).Succeeded);

        await new UserService(users).DeleteAsync(1, new Caller(1, false));

        AuthenticateResult result = await AuthenticateAsync(services, token);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
    }

    [Theory]
    [InlineData("1", false, true)]
    [InlineData("1", true, false)]
    [InlineData("99", false, false)]
    [InlineData("invalid", false, false)]
    [InlineData(null, false, false)]
    public async Task ShouldCheckTheAccount_WhenASignedTokenIsReceived(string? userId, bool deleted, bool allowed)
    {
        FakeUserRepository users = CreateUsers();
        users.Items.Single().IsDeleted = deleted;
        await using ServiceProvider services = CreateServices(users);

        AuthenticateResult result = await AuthenticateAsync(services, CreateToken(userId));

        Assert.Equal(allowed, result.Succeeded);
    }

    private static FakeUserRepository CreateUsers()
    {
        FakeUserRepository users = new();
        users.Seed(new User { Id = 1, UserName = "mads", Password = "secret", Email = "mads@example.test" });
        return users;
    }

    private static ServiceProvider CreateServices(FakeUserRepository users)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSingleton<IUserRepository>(users);
        services.AddScoped<UserTokenEvents>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.EventsType = typeof(UserTokenEvents);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = Key,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services.BuildServiceProvider();
    }

    private static string CreateToken(string? userId)
    {
        List<Claim> claims = [new Claim("Role", "Admin")];
        if (userId is not null)
        {
            claims.Add(new Claim("Id", userId));
        }

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)
        });
    }

    private static async Task<AuthenticateResult> AuthenticateAsync(ServiceProvider services, string token)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        DefaultHttpContext context = new() { RequestServices = scope.ServiceProvider };
        context.Request.Headers.Authorization = $"Bearer {token}";
        return await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
    }
}
