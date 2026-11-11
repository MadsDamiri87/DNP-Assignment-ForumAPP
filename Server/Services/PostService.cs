using ApiContracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using ServiceContracts;

namespace Services;

public class PostService : IPostService
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;
    private readonly ISubForumRepository subForumRepository;
    private readonly ICommentRepository commentRepository;

    public PostService(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ISubForumRepository subForumRepository,
        ICommentRepository commentRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
        this.subForumRepository = subForumRepository;
        this.commentRepository = commentRepository;
    }

    public async Task<PostDto> CreateAsync(CreatePostDto request, int userId)
    {
        ValidateFields(request.Title, request.Body);

        User author = await GetExistingUserAsync(userId);
        await EnsureSubForumExistsAsync(request.SubForumId);

        Post post = new()
        {
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            UserId = userId,
            SubForumId = request.SubForumId,
            CreatedAt = DateTime.UtcNow
        };

        Post created = await postRepository.AddAsync(post);
        return ToDto(created, author.UserName, 0);
    }

    public async Task<PostDto> UpdateAsync(int id, UpdatePostDto request, Caller caller)
    {
        ValidateFields(request.Title, request.Body);

        Post existing = await postRepository.GetSingleAsync(id);
        Ownership.Ensure(existing.UserId, caller, "post");

        await EnsureSubForumExistsAsync(request.SubForumId);

        existing.Title = request.Title.Trim();
        existing.Body = request.Body.Trim();
        existing.SubForumId = request.SubForumId;

        await postRepository.UpdateAsync(existing);

        return ToDto(existing,
            await FindUserNameAsync(existing.UserId),
            await CountCommentsForAsync(existing.Id));
    }

    public async Task DeleteAsync(int id, Caller caller)
    {
        Post existing = await postRepository.GetSingleAsync(id);
        Ownership.Ensure(existing.UserId, caller, "post");

        List<int> commentIds = await commentRepository.GetMany()
            .Where(comment => comment.PostId == id)
            .Select(comment => comment.Id)
            .ToListAsync();

        foreach (int commentId in commentIds)
        {
            await commentRepository.DeleteAsync(commentId);
        }
        await postRepository.DeleteAsync(id);
    }

    public async Task<PostDto> GetSingleAsync(int id)
    {
        Post post = await postRepository.GetSingleAsync(id);
        return ToDto(post, await FindUserNameAsync(post.UserId), await CountCommentsForAsync(post.Id));
    }

    public async Task<IEnumerable<PostDto>> GetManyAsync(
        string? titleContains = null,
        int? userId = null,
        string? userNameContains = null,
        int? subForumId = null)
    {
        IQueryable<Post> posts = postRepository.GetMany();

        if (!string.IsNullOrWhiteSpace(titleContains))
        {
            string filter = titleContains.Trim().ToLower();
            posts = posts.Where(post => post.Title.ToLower().Contains(filter));
        }

        if (userId.HasValue)
        {
            posts = posts.Where(post => post.UserId == userId.Value);
        }

        if (subForumId.HasValue)
        {
            posts = posts.Where(post => post.SubForumId == subForumId.Value);
        }

        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            string filter = userNameContains.Trim().ToLower();

            List<int> matchingUserIds = await userRepository.GetMany()
                .Where(user => user.UserName.ToLower().Contains(filter))
                .Select(user => user.Id)
                .ToListAsync();

            posts = posts.Where(post => matchingUserIds.Contains(post.UserId));
        }

        Dictionary<int, string> userNames = await userRepository.GetMany()
            .ToDictionaryAsync(user => user.Id, user => user.UserName);

        Dictionary<int, int> commentCounts = await commentRepository.GetMany()
            .GroupBy(comment => comment.PostId)
            .Select(group => new { PostId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(group => group.PostId, group => group.Count);

        List<Post> loadedPosts = await posts.ToListAsync();

        return loadedPosts.Select(post => ToDto(
                post,
                userNames.GetValueOrDefault(post.UserId, DeletedUser.UserName),
                commentCounts.GetValueOrDefault(post.Id)))
            .ToList();
    }

    private static void ValidateFields(string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Title and body are required.");
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

    private async Task EnsureSubForumExistsAsync(int? subForumId)
    {
        if (!subForumId.HasValue)
        {
            return;
        }

        try
        {
            await subForumRepository.GetSingleAsync(subForumId.Value);
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException($"SubForum with id '{subForumId.Value}' doesn't exist.");
        }
    }

    private async Task<string> FindUserNameAsync(int userId)
    {
        User? user = await userRepository.GetMany().FirstOrDefaultAsync(candidate => candidate.Id == userId);
        return user?.UserName ?? DeletedUser.UserName;
    }

    private Task<int> CountCommentsForAsync(int postId) =>
        commentRepository.GetMany().CountAsync(comment => comment.PostId == postId);

    private static PostDto ToDto(Post post, string authorUserName, int commentCount) =>
        new(post.Id, post.Title, post.Body, post.UserId, authorUserName, post.SubForumId,
            post.CreatedAt, commentCount);
}
