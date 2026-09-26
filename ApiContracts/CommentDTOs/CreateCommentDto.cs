namespace ApiContracts;

public record CreateCommentDto(string Body, int PostId, int UserId);
