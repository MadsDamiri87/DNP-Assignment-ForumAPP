using ApiContracts;

namespace Blazor_Application.Services;

public interface ISubForumApiService : IHttpService<SubForumDto, CreateSubForumDto, UpdateSubForumDto>
{
    Task<List<SubForumDto>> GetSubForumsAsync(
        string? nameContains = null,
        int? creatorUserId = null);
}
