using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class DataSeeder
{
    private readonly IUserRepository userRepository;
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;
    private readonly ISubForumRepository subForumRepository;

    public DataSeeder(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        ISubForumRepository subForumRepository)
    {
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.subForumRepository = subForumRepository;
    }
    
    public async Task SeedAsync()
    {
        User user1 = await userRepository.AddAsync(new User
        {
            Id = 1,
            UserName = "user1",
            PasswordHash =  "hash1",
            Email = "mads@examp.dk",
            CreatedDate =  DateTime.Now.AddMonths(-3)
        });

        User user2 = await userRepository.AddAsync(new User
        {
            Id = 2,
            UserName = "user2",
            PasswordHash =  "hash2",
            Email = "peter@examp.dk",
            CreatedDate = DateTime.Now.AddMonths(-8)
        });

        SubForum programming = await subForumRepository.AddAsync(new SubForum
        {
            Name = "Programming",
            Description = "News on .NET",
            CreatorUserId = user1.Id,
            DateCreated = DateTime.Now.AddMonths(-2)
        });

        SubForum gaming = await subForumRepository.AddAsync(new SubForum
            {
                Name = "Gaming",
                Description = "Aimbot",
                CreatorUserId = user2.Id,
                DateCreated = DateTime.Now.AddMonths(-1)
            }
        );

        Post post1 = await postRepository.AddAsync(new Post
        {
            UserId = user1.Id,
            SubForumId = programming.Id,
            Title = "How does async work?",
            Body = "I am trying to understand async and Task in C#."
        });
        Post post2 = await postRepository.AddAsync(new Post
        {
            UserId = user2.Id,
            SubForumId = gaming.Id,
            Title = "Favorite game?",
            Body = "What game are you playing right now?"
        });
        
        await commentRepository.AddAsync(new Comment
        {
            PostId = post1.Id,
            UserId = user2.Id,
            Body = "Task represents work that may complete later.",
            Date = DateTime.Now.AddDays(0)
        });

        await commentRepository.AddAsync(new Comment
        {
            PostId = post2.Id,
            UserId = user1.Id,
            Body = "I am playing Baldur's Gate 3 right now.",
            Date = DateTime.Now.AddDays(0)
        });
    }
}