using ApiContracts;

namespace ServiceContracts;

public interface ICommentService : IService<CommentDto, CreateCommentDto, UpdateCommentDto>
{
    IEnumerable<CommentDto> GetMany(
        int? postId = null,
        int? userId = null,
        string? userNameContains = null);
}
