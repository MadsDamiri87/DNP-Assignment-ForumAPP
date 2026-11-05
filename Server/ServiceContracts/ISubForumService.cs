using ApiContracts;

namespace ServiceContracts;

public interface ISubForumService : IAuthoredService<SubForumDto, CreateSubForumDto, UpdateSubForumDto>
{
    IEnumerable<SubForumDto> GetMany(
        string? nameContains = null, 
        int? creatorUserId = null);
}
