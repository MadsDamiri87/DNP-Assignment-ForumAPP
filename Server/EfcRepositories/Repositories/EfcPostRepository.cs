using Entities;
using RepositoryContracts;

namespace EfcRepositories.EFCRepositories;

public class EfcPostRepository : EfcRepositoryBase<Post>, IPostRepository
{
    public EfcPostRepository(AppContext ctx) : base(ctx)
    {
    }
}