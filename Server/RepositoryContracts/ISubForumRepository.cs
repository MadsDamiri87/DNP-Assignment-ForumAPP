using Entities;

namespace RepositoryContracts;

public interface ISubForumRepository
{
    Task<SubForum> AddAsync(SubForum subForum);
    Task UpdateAsync(SubForum subForum);
    Task DeleteAsync(int id);
    Task<SubForum> GetSingleAsync(int id);
    IQueryable<SubForum> GetManyAsync();
}