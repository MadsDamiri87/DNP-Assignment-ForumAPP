using Entities;
using RepositoryContracts;

namespace EfcRepositories.Repositories;

public class EfcUserRepository : EfcRepositoryBase<User>, IUserRepository
{
    public EfcUserRepository(AppContext ctx) : base(ctx)
    {
    }
}