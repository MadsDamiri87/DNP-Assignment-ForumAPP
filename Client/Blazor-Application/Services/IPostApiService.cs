using ApiContracts;

namespace Blazor_Application.Services;

public interface IPostApiService : IHttpService<PostDto, CreatePostDto, UpdatePostDto>
{
    Task<List<PostDto>> GetPostsAsync(
        string? titleContains = null,
        int? userId = null,
        string? userNameContains = null,
        int? subForumId = null);
}
