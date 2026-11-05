namespace ApiContracts;

public record AuthDto
{
    public required string UserName { get; init; }
    
    public required string Password { get; init; }
}