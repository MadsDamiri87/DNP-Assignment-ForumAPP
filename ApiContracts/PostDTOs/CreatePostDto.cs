namespace ApiContracts;

public record CreatePostDto
{
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required int UserId { get; init; }
    public int? SubForumId { get; init; }
}
