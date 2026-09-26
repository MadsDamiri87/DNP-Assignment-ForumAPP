using ApiContracts;

namespace ServiceContracts;

public interface ISubForumService
{
    Task<SubForumDto> CreateAsync(CreateSubForumDto request);
    Task<SubForumDto> UpdateAsync(int id, UpdateSubForumDto request);
    Task DeleteAsync(int id);
    Task<SubForumDto> GetSingleAsync(int id);
    IEnumerable<SubForumDto> GetMany(string? nameContains = null, int? creatorUserId = null);
}
