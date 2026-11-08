namespace Entities;

public class Post : IEntity
{
    public int Id {get; set;}
    public required string Title {get; set;}
    public required string Body {get; set;}
    public required DateTime CreatedAt {get; set;}
    
    public int? SubForumId {get; set;}
    public SubForum? SubForum {get; set;}
    
    public int UserId {get; set;}
    public User? User { get; set;}
    
    public ICollection<Comment> CreatedComments {get; set;} = new List<Comment>();
    
}