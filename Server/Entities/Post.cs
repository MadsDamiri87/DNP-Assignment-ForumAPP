namespace Entities;

public class Post : IEntity
{
    public int Id {get; set;}
    public int SubforumId {get; set;}
    public int UserId {get; set;}
    
    public string Title {get; set;}
    public string Body {get; set;}
    
    
    
    
}