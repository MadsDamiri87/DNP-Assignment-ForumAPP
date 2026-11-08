namespace Entities;

public class User : IEntity
{
    public int Id {get; set;}
    public required string UserName {get; set;}
    public required string Password {get; set;}
    public required string Email {get; set;}
    public string Role { get; set; } = "User";
    public DateTime CreatedAt {get; set;}
    
    public bool IsDeleted {get; set;}
    
    public ICollection<Post> CreatedPosts {get; set;} = new List<Post>();
    public ICollection<SubForum> CreatedSubForums {get; set;} = new List<SubForum>();
    public ICollection<Comment> CreatedComments {get; set;} = new List<Comment>();
}