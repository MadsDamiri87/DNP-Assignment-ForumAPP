namespace Entities;

public class SubForum : IEntity
{
    public int Id {get; init;}
    public required string Name {get; set;}
    public int CreatorUserId {get; set;}
    public required string Description {get; set;}
    public DateTime DateCreated {get; set;}
    
}