using ApiContracts;
using Entities;
using Microsoft.EntityFrameworkCore;
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

    public async Task<CommentDto> CreateAsync(CreateCommentDto request, int userId)
    {
        ValidateBody(request.Body);

        await EnsurePostExistsAsync(request.PostId);
        User author = await GetExistingUserAsync(userId);

        Comment comment = new()
        {
            Body = request.Body.Trim(),
            PostId = request.PostId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        Comment created = await commentRepository.AddAsync(comment);
        return ToDto(created, author.UserName);
    }

    public async Task<CommentDto> UpdateAsync(int id, UpdateCommentDto request, Caller caller)
    {
        ValidateBody(request.Body);

        Comment existing = await commentRepository.GetSingleAsync(id);
        Ownership.Ensure(existing.UserId, caller, "comment");

        existing.Body = request.Body.Trim();

        await commentRepository.UpdateAsync(existing);
        return ToDto(existing, await FindUserNameAsync(existing.UserId));
    }

    public async Task DeleteAsync(int id, Caller caller)
    {
        Comment existing = await commentRepository.GetSingleAsync(id);
        Ownership.Ensure(existing.UserId, caller, "comment");

        await commentRepository.DeleteAsync(id);
    }

    public async Task<CommentDto> GetSingleAsync(int id)
    {
        Comment comment = await commentRepository.GetSingleAsync(id);
        return ToDto(comment, await FindUserNameAsync(comment.UserId));
    }

    public async Task<IEnumerable<CommentDto>> GetManyAsync(
        int? postId = null,
        int? userId = null,
        string? userNameContains = null)
    {
        IQueryable<Comment> comments = commentRepository.GetMany();

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
            string filter = userNameContains.Trim().ToLower();

            List<int> matchingUserIds = await userRepository.GetMany()
                .Where(user => !user.IsDeleted && user.UserName.ToLower().Contains(filter))
                .Select(user => user.Id)
                .ToListAsync();

            comments = comments.Where(comment => matchingUserIds.Contains(comment.UserId));
        }

        Dictionary<int, string> userNames = await userRepository.GetMany()
            .Where(user => !user.IsDeleted)
            .ToDictionaryAsync(user => user.Id, user => user.UserName);

        List<Comment> loadComments = await comments.ToListAsync();


        return loadComments
            .Select(comment => ToDto(comment, userNames.GetValueOrDefault(comment.UserId, DeletedUser.UserName)))
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
        User? user = await userRepository.GetMany()
            .FirstOrDefaultAsync(candidate => candidate.Id == userId && !candidate.IsDeleted);

        return user ?? throw new ArgumentException($"User with id '{userId}' doesn't exist.");
    }

    private async Task<string> FindUserNameAsync(int userId)
    {
        User? user = await userRepository.GetMany()
            .FirstOrDefaultAsync(candidate => candidate.Id == userId && !candidate.IsDeleted);
        return user?.UserName ?? DeletedUser.UserName;
    }

    private static CommentDto ToDto(Comment comment, string authorUserName) =>
        new(comment.Id, comment.Body, comment.PostId, comment.UserId, authorUserName, comment.CreatedAt);
}
