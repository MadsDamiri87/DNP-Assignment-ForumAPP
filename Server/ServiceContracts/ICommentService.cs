using ApiContracts;

namespace ServiceContracts;

public interface ICommentService : IAuthoredService<CommentDto, CreateCommentDto, UpdateCommentDto>
{
    IEnumerable<CommentDto> GetMany(
        int? postId = null,
        int? userId = null,
        string? userNameContains = null);
}
