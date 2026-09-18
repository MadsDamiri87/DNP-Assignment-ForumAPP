using Entities;
using RepositoryContracts;

namespace FileRepository;

public class PostFileRepository : FileRepositoryBase<Post>, IPostRepository
{
    public PostFileRepository(string filePath = "posts.json") : base(filePath)
    {
    }
}
