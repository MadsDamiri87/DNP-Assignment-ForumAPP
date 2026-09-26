namespace ApiContracts;

public record UpdateCommentDto
{
    public required string Body { get; init; }
}
