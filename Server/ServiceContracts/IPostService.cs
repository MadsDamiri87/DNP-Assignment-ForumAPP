using ApiContracts;

namespace ServiceContracts;

public interface IPostService
{
    Task<PostDto> CreateAsync(CreatePostDto request);
    Task<PostDto> UpdateAsync(int id, UpdatePostDto request);
    Task DeleteAsync(int id);
    Task<PostDto> GetSingleAsync(int id);
    IEnumerable<PostDto> GetMany(
        string? titleContains = null,
        int? userId = null,
        string? userNameContains = null,
        int? subForumId = null);
}
