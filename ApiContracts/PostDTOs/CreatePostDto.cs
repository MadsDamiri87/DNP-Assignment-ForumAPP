namespace ApiContracts;

public record CreatePostDto(string Title, string Body, int UserId, int? SubForumId = null);
