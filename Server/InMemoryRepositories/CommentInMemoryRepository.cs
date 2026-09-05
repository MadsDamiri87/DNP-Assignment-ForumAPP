using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class CommentInMemoryRepository 
    : RepositoryBase<Comment>, ICommentRepository
{
    
    // Dummy Data:
    public CommentInMemoryRepository()
    {
        entities.AddRange(new[]
        {
            new Comment
            {
                Id = 1,
                PostId = 1,
                UserId = 2,
                Body = "Task represents work that may complete later.",
                Date = DateTime.Now.AddDays(-2)
            },
            new Comment
            {
                Id = 2,
                PostId = 2,
                UserId = 1,
                Body = "I am playing Baldur's Gate 3 right now.",
                Date = DateTime.Now.AddDays(-1)
            }
        });
    }
}