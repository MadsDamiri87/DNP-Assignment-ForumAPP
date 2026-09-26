namespace ApiContracts;

public record UpdateUserDto
{
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required string Email { get; init; }
}
