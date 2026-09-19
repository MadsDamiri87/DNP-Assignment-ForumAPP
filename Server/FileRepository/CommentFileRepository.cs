using Entities;
using RepositoryContracts;

namespace FileRepository;

public class CommentFileRepository : FileRepositoryBase<Comment>, ICommentRepository
{
    public CommentFileRepository(string filePath = DataFiles.Comments) : base(filePath)
    {
    }
}
