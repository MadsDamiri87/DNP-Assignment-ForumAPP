using Entities;
using RepositoryContracts;

namespace EfcRepositories.Repositories;

public class EfcSubForumRepository : EfcRepositoryBase<SubForum>, ISubForumRepository
{
    public EfcSubForumRepository(AppContext ctx) : base(ctx)
    {
    }
}