using System.Reflection;
using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class SinglePostView
{
    private IPostRepository postRepository;
    private ICommentRepository commentRepository;
    
    public SinglePostView(IPostRepository postRepository,  ICommentRepository commentRepository)
    {
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
    }


    public async Task ShowPostAsync()
    {
        Console.Write("Enter ID: ");
        string postId = Console.ReadLine()?.Trim() ?? "";

        if (!int.TryParse(postId, out int id))
        {
            Console.Write("Invalid ID: ");
            return;
        }

        Post showPost = await postRepository.GetSingleAsync(id);

        Console.WriteLine();
        Console.WriteLine($"Title: {showPost.Title}");
        Console.WriteLine($"Body: {showPost.Body}");
        
        Console.WriteLine();
        Console.WriteLine("Comments:");
        
        IQueryable<Comment> comments = commentRepository.GetManyAsync();
        
        foreach (Comment comment in comments)
        {
            if (comment.PostId == showPost.Id)
            {
                Console.WriteLine($"- {comment.Body}");
            }
        }

    }
}