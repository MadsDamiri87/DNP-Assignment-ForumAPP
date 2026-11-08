using ApiContracts;

namespace Blazor_Application.Services;

public class HttpPostApiService : HttpServiceBase<PostDto, CreatePostDto, UpdatePostDto>, IPostApiService
{
    public HttpPostApiService(HttpClient client) : base(client, "posts")
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
