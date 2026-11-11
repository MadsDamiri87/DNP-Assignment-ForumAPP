using ApiContracts;

namespace ServiceContracts;

public interface ICommentService : IAuthoredService<CommentDto, CreateCommentDto, UpdateCommentDto>
{
    Task<IEnumerable<CommentDto>> GetManyAsync(
        int? postId = null,
        int? userId = null,
        string? userNameContains = null);
}
