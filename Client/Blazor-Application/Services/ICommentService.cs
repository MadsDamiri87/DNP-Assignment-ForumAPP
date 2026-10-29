using ApiContracts;

namespace Blazor_Application.Services;

public interface ICommentService : IHttpService<CommentDto, CreateCommentDto, UpdateCommentDto>
{
    Task<List<CommentDto>> GetCommentsAsync(
        int? postId = null,
        int? userId = null,
        string? userNameContains = null);
}
