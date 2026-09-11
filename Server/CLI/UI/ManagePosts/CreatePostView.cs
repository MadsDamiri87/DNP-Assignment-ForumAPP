using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class CreatePostView
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public CreatePostView(IPostRepository postRepository, IUserRepository userRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }

    
    private bool CheckUserExists(int userId)
    {
        bool userExists = userRepository.GetManyAsync().Any(user => user.Id == userId);
        
        if (!userExists)
        {
            Console.WriteLine($"User with id {userId} not found");
            return false;
        }

        return true;
    }

    public async Task CreatePostAsync()
    {
        Console.Write("Title: ");
        string title = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Body: ");
        string body = Console.ReadLine()?.Trim() ?? "";

        Console.Write("User ID: ");
        string userIdInput = Console.ReadLine()?.Trim() ?? "";
        
        
        if (!int.TryParse(userIdInput, out int userId))
        {
            Console.WriteLine("Invalid User ID");
            return;
        }

        if (CheckUserExists(userId))
        {
            Console.WriteLine($"User with id '{userId}' don't exists");
            return;
        }
        
        Post post = new Post
        {
            Title = title,
            Body = body,
            UserId = userId,
        };
        
        Post createdPost = await postRepository.AddAsync(post);
        
        Console.WriteLine($"Post created with id: {createdPost.Id}");
        
    }
        
}