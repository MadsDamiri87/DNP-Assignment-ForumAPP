using ApiContracts;
using Entities;
using RepositoryContracts;
using ServiceContracts;

namespace Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository commentRepository;
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserRepository userRepository)
    {
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }

    public async Task<CommentDto> CreateAsync(CreateCommentDto request)
    {
        ValidateBody(request.Body);

        await EnsurePostExistsAsync(request.PostId);
        User author = await GetExistingUserAsync(request.UserId);

        Comment comment = new()
        {
            Body = request.Body.Trim(),
            PostId = request.PostId,
            UserId = request.UserId,
            CreatedAt = DateTime.Now
        };

        Comment created = await commentRepository.AddAsync(comment);
        return ToDto(created, author.UserName);
    }

    public async Task<CommentDto> UpdateAsync(int id, UpdateCommentDto request)
    {
        ValidateBody(request.Body);

        Comment existing = await commentRepository.GetSingleAsync(id);

        Comment updated = new()
        {
            Id = existing.Id,
            Body = request.Body.Trim(),
            PostId = existing.PostId,
            UserId = existing.UserId,
            CreatedAt = existing.CreatedAt
        };

        await commentRepository.UpdateAsync(updated);
        return ToDto(updated, FindUserName(updated.UserId));
    }

    public Task DeleteAsync(int id)
    {
        return commentRepository.DeleteAsync(id);
    }

    public async Task<CommentDto> GetSingleAsync(int id)
    {
        Comment comment = await commentRepository.GetSingleAsync(id);
        return ToDto(comment, FindUserName(comment.UserId));
    }

    public IEnumerable<CommentDto> GetMany(
        int? postId = null,
        int? userId = null,
        string? userNameContains = null)
    {
        IQueryable<Comment> comments = commentRepository.GetManyAsync();

        if (postId.HasValue)
        {
            comments = comments.Where(comment => comment.PostId == postId.Value);
        }

        if (userId.HasValue)
        {
            comments = comments.Where(comment => comment.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            string filter = userNameContains.Trim();
            List<int> matchingUserIds = userRepository.GetManyAsync()
                .Where(user => user.UserName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(user => user.Id)
                .ToList();

            comments = comments.Where(comment => matchingUserIds.Contains(comment.UserId));
        }

        Dictionary<int, string> userNames = userRepository.GetManyAsync()
            .ToDictionary(user => user.Id, user => user.UserName);

        return comments
            .Select(comment => ToDto(comment, userNames.GetValueOrDefault(comment.UserId, "")))
            .ToList();
    }

    private static void ValidateBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Body is required.");
        }
    }

    private async Task EnsurePostExistsAsync(int postId)
    {
        try
        {
            await postRepository.GetSingleAsync(postId);
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException($"Post with id '{postId}' doesn't exist.");
        }
    }

    private async Task<User> GetExistingUserAsync(int userId)
    {
        try
        {
            return await userRepository.GetSingleAsync(userId);
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException($"User with id '{userId}' doesn't exist.");
        }
    }

    private string FindUserName(int userId)
    {
        User? user = userRepository.GetManyAsync().FirstOrDefault(candidate => candidate.Id == userId);
        return user?.UserName ?? "";
    }

    private static CommentDto ToDto(Comment comment, string authorUserName) =>
        new(comment.Id, comment.Body, comment.PostId, comment.UserId, authorUserName, comment.CreatedAt);
}
