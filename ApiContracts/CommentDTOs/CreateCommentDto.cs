namespace ApiContracts;

public record CreateCommentDto
{
    public required string Body { get; init; }
    public required int PostId { get; init; }
    public required int UserId { get; init; }
}
