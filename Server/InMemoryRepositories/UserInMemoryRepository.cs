using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class UserInMemoryRepository 
    : RepositoryBase<User>, IUserRepository
{
    // Dummy Data: 
    public UserInMemoryRepository()
    {
        entities.AddRange(new[]
        {
            new User
            {
                Id = 1,
                UserName = "mads",
                PasswordHash = "hash1",
                Email = "mads@example.com",
                CreatedDate = DateTime.Now.AddMonths(-6)
            },
            new User
            {
                Id = 2,
                UserName = "anna",
                PasswordHash = "hash2",
                Email = "anna@example.com",
                CreatedDate = DateTime.Now.AddMonths(-3)
            }
        });
    }
}