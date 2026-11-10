using Entities;
using RepositoryContracts;

namespace EfcRepositories.EFCRepositories;

public class EfcUserRepository : EfcRepositoryBase<User>, IUserRepository
{
    public EfcUserRepository(AppContext ctx) : base(ctx)
    {
    }
}