namespace ApiContracts;

public record CommentDto(
    int Id,
    string Body,
    int PostId,
    int UserId,
    string AuthorUserName,
    DateTime CreatedAt);
