using ApiContracts;

namespace Blazor_Application.Services;

public class HttpCommentService : HttpServiceBase<CommentDto, CreateCommentDto, UpdateCommentDto>, ICommentService
{
    public HttpCommentService(HttpClient client) : base(client, "comments")
    {
    }

    public Task<List<CommentDto>> GetCommentsAsync(
        int? postId = null,
        int? userId = null,
        string? userNameContains = null) =>
        GetManyAsync(
            ("postId", postId),
            ("userId", userId),
            ("userNameContains", userNameContains));
}

