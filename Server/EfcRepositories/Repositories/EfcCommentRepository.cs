using Entities;
using RepositoryContracts;

namespace EfcRepositories.Repositories;

public class EfcCommentRepository : EfcRepositoryBase<Comment>, ICommentRepository
{
    public EfcCommentRepository(AppContext ctx) : base(ctx)
    {
    }
}