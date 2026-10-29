using ApiContracts;

namespace Blazor_Application.Services;

public class HttpPostService : HttpServiceBase<PostDto, CreatePostDto, UpdatePostDto>, IPostService
{
    public HttpPostService(HttpClient client) : base(client, "posts")
    {
    }

    public Task<List<PostDto>> GetPostsAsync(
        string? titleContains = null,
        int? userId = null,
        string? userNameContains = null,
        int? subForumId = null) =>
        GetManyAsync(
            ("titleContains", titleContains),
            ("userId", userId),
            ("userNameContains", userNameContains),
            ("subForumId", subForumId));
}
