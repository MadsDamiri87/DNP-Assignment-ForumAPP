using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class PostInMemoryRepository 
    : RepositoryBase<Post>, IPostRepository
{
    // Dummy Data:
    public PostInMemoryRepository()
    {
        entities.AddRange(new[]
        {
            new Post
            {
                Id = 1,
                UserId = 1,
                SubForumId = 1,
                Title = "How does async work?",
                Body = "I am trying to understand async and Task in C#."
            },
            new Post
            {
                Id = 2,
                UserId = 2,
                SubForumId = 2,
                Title = "Favorite game?",
                Body = "What game are you playing right now?"
            }
        });
    }
}