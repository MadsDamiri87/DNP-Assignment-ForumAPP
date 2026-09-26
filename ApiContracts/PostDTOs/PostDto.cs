namespace ApiContracts;

public record PostDto(
    int Id,
    string Title,
    string Body,
    int UserId,
    string AuthorUserName,
    int? SubForumId,
    DateTime CreatedAt);
