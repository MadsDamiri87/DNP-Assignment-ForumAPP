namespace Entities;

public class Comment : IEntity
{
    public int Id {get; set;}
    public required string Body {get; set;}
    public DateTime CreatedAt {get; set;}
    
    
    public int PostId {get; set;}
    public Post? Post {get; set;}
    public int UserId {get; set;}
    public User? User {get; set;}
    
}