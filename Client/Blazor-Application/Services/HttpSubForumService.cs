using ApiContracts;

namespace Blazor_Application.Services;

public class HttpSubForumService : HttpServiceBase<SubForumDto, CreateSubForumDto, UpdateSubForumDto>, ISubForumService
{
    public HttpSubForumService(HttpClient client) : base(client, "subforums")
    {
    }

    public Task<List<SubForumDto>> GetSubForumsAsync(
        string? nameContains = null,
        int? creatorUserId = null) =>
        GetManyAsync(
            ("nameContains", nameContains),
            ("creatorUserId", creatorUserId));
}
