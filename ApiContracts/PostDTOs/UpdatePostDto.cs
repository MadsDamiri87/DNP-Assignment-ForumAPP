namespace ApiContracts;

public record UpdatePostDto
{
    public required string Title { get; init; }
    public required string Body { get; init; }
    public int? SubForumId { get; init; }
}
