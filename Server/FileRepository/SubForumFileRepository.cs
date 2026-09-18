using Entities;
using RepositoryContracts;

namespace FileRepository;

public class SubForumFileRepository : FileRepositoryBase<SubForum>, ISubForumRepository
{
    public SubForumFileRepository(string filePath = "subforums.json") : base(filePath)
    {
    }
}
