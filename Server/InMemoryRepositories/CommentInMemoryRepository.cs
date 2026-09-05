using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class CommentInMemoryRepository 
    : RepositoryBase<Comment>, ICommentRepository
{

}