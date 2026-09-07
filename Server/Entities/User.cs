namespace Entities;

public class User : IEntity
{
    public int Id {get; init;}
    public required string UserName {get; set;}
    public required string PasswordHash {get; set;}
    public required string Email {get; set;}
    public DateTime CreatedDate {get; set;}
    
}