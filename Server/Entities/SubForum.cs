using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

public class SubForum : IEntity
{
    public int Id {get; set;}
    public required string Name {get; set;}
    public required string Description {get; set;}
    public DateTime CreatedAt {get; set;}
    
    public int CreatorUserId {get; set;}
    
    [ForeignKey(nameof(CreatorUserId))]
    public User? CreatorUser { get; set; }

    public ICollection<Post> CreatedPosts {get; set;} = new List<Post>();

}