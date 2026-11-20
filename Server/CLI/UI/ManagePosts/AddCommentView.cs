using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class AddCommentView
{
    private readonly ICommentRepository commentRepository;
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public AddCommentView(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserRepository userRepository)
    {
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }

    public async Task AddCommentAsync()
    {
        Console.WriteLine("Add comment selected");

        Console.Write("Post ID: ");
        string postIdInput = Console.ReadLine()?.Trim() ?? "";

        Console.Write("User ID: ");
        string userIdInput = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Body: ");
        string body = Console.ReadLine()?.Trim() ?? "";

        if (!int.TryParse(postIdInput, out int postId))
        {
            Console.WriteLine("Invalid Post ID");
            return;
        }

        if (!int.TryParse(userIdInput, out int userId))
        {
            Console.WriteLine("Invalid User ID");
            return;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            Console.WriteLine("Body is required");
            return;
        }

        if (!postRepository.GetMany().Any(post => post.Id == postId))
        {
            Console.WriteLine($"Post with id '{postId}' doesn't exist");
            return;
        }

        if (!userRepository.GetMany().Any(user => user.Id == userId))
        {
            Console.WriteLine($"User with id '{userId}' doesn't exist");
            return;
        }

        Comment comment = new Comment
        {
            PostId = postId,
            UserId = userId,
            Body = body,
            CreatedAt = DateTime.UtcNow
        };

        Comment createdComment = await commentRepository.AddAsync(comment);

        Console.WriteLine($"Comment created with id: {createdComment.Id}");
    }
}
