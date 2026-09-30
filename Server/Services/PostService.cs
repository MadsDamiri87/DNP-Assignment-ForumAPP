using ApiContracts;
using Entities;
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

    public async Task<PostDto> CreateAsync(CreatePostDto request)
    {
        ValidateFields(request.Title, request.Body);

        User author = await GetExistingUserAsync(request.UserId);
        await EnsureSubForumExistsAsync(request.SubForumId);

        Post post = new()
        {
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            UserId = request.UserId,
            SubForumId = request.SubForumId,
            CreatedAt = DateTime.UtcNow
        };

        Post created = await postRepository.AddAsync(post);
        return ToDto(created, author.UserName);
    }

    public async Task<PostDto> UpdateAsync(int id, UpdatePostDto request)
    {
        ValidateFields(request.Title, request.Body);

        Post existing = await postRepository.GetSingleAsync(id);
        await EnsureSubForumExistsAsync(request.SubForumId);

        Post updated = new()
        {
            Id = existing.Id,
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            UserId = existing.UserId,
            SubForumId = request.SubForumId,
            CreatedAt = existing.CreatedAt
        };

        await postRepository.UpdateAsync(updated);
        return ToDto(updated, FindUserName(updated.UserId));
    }

    public async Task DeleteAsync(int id)
    {

        List<int> commentIds = commentRepository.GetMany()
            .Where(comment => comment.PostId == id)
            .Select(comment => comment.Id)
            .ToList();

        foreach (int commentId in commentIds)
        {
            await commentRepository.DeleteAsync(commentId);
        }
        await postRepository.DeleteAsync(id);
    }

    public async Task<PostDto> GetSingleAsync(int id)
    {
        Post post = await postRepository.GetSingleAsync(id);
        return ToDto(post, FindUserName(post.UserId));
    }

    public IEnumerable<PostDto> GetMany(
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
            List<int> matchingUserIds = userRepository.GetMany()
                .Where(user => user.UserName.ToLower().Contains(filter))
                .Select(user => user.Id)
                .ToList();

            posts = posts.Where(post => matchingUserIds.Contains(post.UserId));
        }

        Dictionary<int, string> userNames = userRepository.GetMany()
            .ToDictionary(user => user.Id, user => user.UserName);

        return posts
            .Select(post => ToDto(post, userNames.GetValueOrDefault(post.UserId, DeletedUser.UserName)))
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

    private string FindUserName(int userId)
    {
        User? user = userRepository.GetMany().FirstOrDefault(candidate => candidate.Id == userId);
        return user?.UserName ?? DeletedUser.UserName;
    }

    private static PostDto ToDto(Post post, string authorUserName) =>
        new(post.Id, post.Title, post.Body, post.UserId, authorUserName, post.SubForumId, post.CreatedAt);
}
