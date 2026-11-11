using ApiContracts;

namespace ServiceContracts;

public interface ISubForumService : IAuthoredService<SubForumDto, CreateSubForumDto, UpdateSubForumDto>
{
    Task<IEnumerable<SubForumDto>> GetManyAsync(
        string? nameContains = null, 
        int? creatorUserId = null);
}
