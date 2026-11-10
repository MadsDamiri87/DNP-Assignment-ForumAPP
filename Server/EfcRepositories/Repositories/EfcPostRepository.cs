using Entities;
using RepositoryContracts;

namespace EfcRepositories.Repositories;

public class EfcPostRepository : EfcRepositoryBase<Post>, IPostRepository
{
    public EfcPostRepository(AppContext ctx) : base(ctx)
    {
    }
}