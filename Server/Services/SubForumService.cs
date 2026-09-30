using ApiContracts;
using Entities;
using RepositoryContracts;
using ServiceContracts;

namespace Services;

public class SubForumService : ISubForumService
{
    private readonly ISubForumRepository subForumRepository;
    private readonly IUserRepository userRepository;
    private readonly IPostRepository postRepository;

    public SubForumService(
        ISubForumRepository subForumRepository,
        IUserRepository userRepository,
        IPostRepository postRepository)
    {
        this.subForumRepository = subForumRepository;
        this.userRepository = userRepository;
        this.postRepository = postRepository;
    }

    public async Task<SubForumDto> CreateAsync(CreateSubForumDto request)
    {
        ValidateFields(request.Name, request.Description);
        EnsureNameIsAvailable(request.Name);

        User creator = await GetExistingUserAsync(request.CreatorUserId);

        SubForum subForum = new()
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            CreatorUserId = request.CreatorUserId,
            CreatedAt = DateTime.UtcNow
        };

        SubForum created = await subForumRepository.AddAsync(subForum);
        return ToDto(created, creator.UserName);
    }

    public async Task<SubForumDto> UpdateAsync(int id, UpdateSubForumDto request)
    {
        ValidateFields(request.Name, request.Description);

        SubForum existing = await subForumRepository.GetSingleAsync(id);
        EnsureNameIsAvailable(request.Name, id);

        SubForum updated = new()
        {
            Id = existing.Id,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            CreatorUserId = existing.CreatorUserId,
            CreatedAt = existing.CreatedAt
        };

        await subForumRepository.UpdateAsync(updated);
        return ToDto(updated, FindUserName(updated.CreatorUserId));
    }

    public async Task DeleteAsync(int id)
    {
        List<Post> postsInSubForum = postRepository.GetMany()
            .Where(post => post.SubForumId == id)
            .ToList();

        foreach (Post post in postsInSubForum)
        {
            post.SubForumId = null;
            await postRepository.UpdateAsync(post);
        }
        await subForumRepository.DeleteAsync(id);
    }

    public async Task<SubForumDto> GetSingleAsync(int id)
    {
        SubForum subForum = await subForumRepository.GetSingleAsync(id);
        return ToDto(subForum, FindUserName(subForum.CreatorUserId));
    }

    public IEnumerable<SubForumDto> GetMany(string? nameContains = null, int? creatorUserId = null)
    {
        IQueryable<SubForum> subForums = subForumRepository.GetMany();

        if (!string.IsNullOrWhiteSpace(nameContains))
        {
            string filter = nameContains.Trim().ToLower();
            subForums = subForums.Where(subForum => subForum.Name.ToLower().Contains(filter));
        }

        if (creatorUserId.HasValue)
        {
            subForums = subForums.Where(subForum => subForum.CreatorUserId == creatorUserId.Value);
        }

        Dictionary<int, string> userNames = userRepository.GetMany()
            .ToDictionary(user => user.Id, user => user.UserName);

        return subForums
            .Select(subForum => ToDto(subForum, userNames.GetValueOrDefault(subForum.CreatorUserId, DeletedUser.UserName)))
            .ToList();
    }

    private static void ValidateFields(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Name and description are required.");
        }
    }

    private void EnsureNameIsAvailable(string name, int? ignoredSubForumId = null)
    {
        string trimmed = name.Trim();

        bool taken = subForumRepository.GetMany()
            .Any(subForum => subForum.Id != ignoredSubForumId
                             && subForum.Name.ToLower() == trimmed.ToLower());

        if (taken)
        {
            throw new ArgumentException($"SubForum: '{trimmed}' already exists.");
        }
    }

    private async Task<User> GetExistingUserAsync(int userId)
    {
        try
        {
            return await userRepository.GetSingleAsync(userId);
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException($"User with id: '{userId}' doesn't exist.");
        }
    }

    private string FindUserName(int userId)
    {
        User? user = userRepository.GetMany().FirstOrDefault(candidate => candidate.Id == userId);
        return user?.UserName ?? DeletedUser.UserName;
    }

    private static SubForumDto ToDto(SubForum subForum, string creatorUserName) =>
        new(subForum.Id, subForum.Name, subForum.Description, subForum.CreatorUserId, creatorUserName, subForum.CreatedAt);
}
