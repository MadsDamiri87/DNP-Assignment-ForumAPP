namespace ApiContracts;

public record UpdatePostDto
{
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required int? SubForumId { get; init; }
}
