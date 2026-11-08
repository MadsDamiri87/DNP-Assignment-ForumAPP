using ApiContracts;

namespace Blazor_Application.Services;

public class HttpSubForumApiService : HttpServiceBase<SubForumDto, CreateSubForumDto, UpdateSubForumDto>, ISubForumApiService
{
    public HttpSubForumApiService(HttpClient client) : base(client, "subforums")
    {
    }

    public Task<List<SubForumDto>> GetSubForumsAsync(
        string? nameContains = null,
        int? creatorUserId = null) =>
        GetManyAsync(
            ("nameContains", nameContains),
            ("creatorUserId", creatorUserId));
}
