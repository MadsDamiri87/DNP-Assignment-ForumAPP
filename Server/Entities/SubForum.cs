namespace Entities;

public class SubForum : IEntity
{
    public int Id {get; set;}
    public string SubForumName {get; set;}
    public int CreatorUserId {get; set;}
    public DateTime DateCreated {get; set;}
    
}