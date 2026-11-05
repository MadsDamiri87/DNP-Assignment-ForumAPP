namespace ApiContracts;

public record UserDto(int Id, string UserName, string Email, string Role, DateTime CreatedAt);
