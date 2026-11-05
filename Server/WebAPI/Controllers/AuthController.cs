using System.Security.Claims;
using System.Text;
using ApiContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ServiceContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IAuthService authService, IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login([FromBody] AuthDto request)
    {
        UserDto? user = await authService.ValidateUserAsync(request);

        if (user == null)
        {
            return Unauthorized("Invalid username or password");
        }

        return Ok(GenerateJwt(user));
    }

    private string GenerateJwt(UserDto user)
    {
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(GenerateClaims(user)),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }

    private static List<Claim> GenerateClaims(UserDto user) =>
    [
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("Id", user.Id.ToString()),
        new Claim("Username", user.UserName),
        new Claim("Email", user.Email),
        new Claim("Role", user.Role)
    ];
}