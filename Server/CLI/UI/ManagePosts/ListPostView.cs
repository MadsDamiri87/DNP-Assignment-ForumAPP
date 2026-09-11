using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ListPostView
{
    private readonly IPostRepository postRepository;
    
    public  ListPostView(IPostRepository postRepository)
    {
        this.postRepository = postRepository;
    }

    public Task ShowPostsAsync()
    {
        IQueryable<Entities.Post> posts = postRepository.GetManyAsync();

        foreach (var post in posts)
        {
            Console.WriteLine($"ID: {post.Id}");
            Console.WriteLine($"Title: {post.Title}");
            Console.WriteLine();
        }

        return Task.CompletedTask;
    }
}