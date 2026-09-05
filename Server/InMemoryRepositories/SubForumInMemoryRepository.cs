using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class SubForumInMemoryRepository 
    : RepositoryBase<SubForum>, ISubForumRepository
{
    // Dummy Data:
    public SubForumInMemoryRepository()
    {
        entities.AddRange(new[]
        {
            new SubForum
            {
                Id = 1,
                SubForumName = "Programming",
                CreatorUserId = 1,
                DateCreated = DateTime.Now.AddMonths(-5)
            },
            new SubForum
            {
                Id = 2,
                SubForumName = "Gaming",
                CreatorUserId = 2,
                DateCreated = DateTime.Now.AddMonths(-2)
            }
        });
    }
}