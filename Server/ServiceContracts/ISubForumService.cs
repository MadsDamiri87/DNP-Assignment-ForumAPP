using ApiContracts;

namespace ServiceContracts;

public interface ISubForumService : IService<SubForumDto, CreateSubForumDto, UpdateSubForumDto>
{
    IEnumerable<SubForumDto> GetMany(
        string? nameContains = null, 
        int? creatorUserId = null);
}
