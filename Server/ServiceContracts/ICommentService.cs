using ApiContracts;

namespace ServiceContracts;

public interface ICommentService
{
    Task<CommentDto> CreateAsync(CreateCommentDto request);
    Task<CommentDto> UpdateAsync(int id, UpdateCommentDto request);
    Task DeleteAsync(int id);
    Task<CommentDto> GetSingleAsync(int id);
    IEnumerable<CommentDto> GetMany(
        int? postId = null,
        int? userId = null,
        string? userNameContains = null);
}
