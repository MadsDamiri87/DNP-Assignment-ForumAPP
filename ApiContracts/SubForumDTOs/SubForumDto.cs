namespace ApiContracts;

public record SubForumDto(
    int Id,
    string Name,
    string Description,
    int CreatorUserId,
    string CreatorUserName,
    DateTime CreatedAt);
