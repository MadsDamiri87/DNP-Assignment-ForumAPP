using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class UserInMemoryRepository 
    : RepositoryBase<User>, IUserRepository
{

}