namespace ApiContracts;

public record UpdateSubForumDto
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}
