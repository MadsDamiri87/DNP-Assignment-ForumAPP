using Entities;
using RepositoryContracts;

namespace FileRepository;

public class UserFileRepository : FileRepositoryBase<User>, IUserRepository
{
    public UserFileRepository(string filePath = "users.json") : base(filePath)
    {
    }
}
