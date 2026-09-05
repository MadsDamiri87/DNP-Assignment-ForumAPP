namespace Entities;

public class Comment : IEntity
{
    public int Id {get; set;}
    public int PostId {get; set;}
    public int User {get; set;}
    
    public string Body {get; set;}
    public DateTime Date {get; set;}
    
    
    
    
}