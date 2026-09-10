namespace Entities;

public class Comment : IEntity
{
    public int Id {get; set;}
    public int PostId {get; set;}
    public int UserId {get; set;}
    public required string Body {get; set;}
    public DateTime Date {get; set;}
    
    
    
    
}