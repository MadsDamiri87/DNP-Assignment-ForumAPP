using ApiContracts;

namespace ServiceContracts;

public interface IPostService : IAuthoredService<PostDto, CreatePostDto, UpdatePostDto>
{
    Task<IEnumerable<PostDto>> GetManyAsync(
        string? titleContains = null,
        int? userId = null,
        string? userNameContains = null,
        int? subForumId = null);
}
