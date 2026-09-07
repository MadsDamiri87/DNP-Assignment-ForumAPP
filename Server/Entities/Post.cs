namespace Entities;

public class Post : IEntity
{
    public int Id {get; init;}
    public int SubForumId {get; set;}
    public int UserId {get; set;}
    
    public required string Title {get; set;}
    public required string Body {get; set;}
    
    
    
    
}