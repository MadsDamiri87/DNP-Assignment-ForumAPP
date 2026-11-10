using Entities;
using RepositoryContracts;

namespace EfcRepositories.EFCRepositories;

public class EfcCommentRepository : EfcRepositoryBase<Comment>, ICommentRepository
{
    public EfcCommentRepository(AppContext ctx) : base(ctx)
    {
    }
}