using ApiContracts;

namespace Blazor_Application.Services;

public class HttpCommentApiService : HttpServiceBase<CommentDto, CreateCommentDto, UpdateCommentDto>, ICommentApiService
{
    public HttpCommentApiService(HttpClient client) : base(client, "comments")
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

