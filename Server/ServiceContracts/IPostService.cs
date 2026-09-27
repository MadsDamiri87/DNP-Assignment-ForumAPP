using ApiContracts;

namespace ServiceContracts;

public interface IPostService : IService<PostDto, CreatePostDto, UpdatePostDto>
{
    IEnumerable<PostDto> GetMany(
        string? titleContains = null,
        int? userId = null,
        string? userNameContains = null,
        int? subForumId = null);
}
