namespace ApiContracts;

public record CreateSubForumDto
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required int CreatorUserId { get; init; }
}
