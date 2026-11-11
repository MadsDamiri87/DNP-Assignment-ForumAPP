using ApiContracts;
using Entities;
using Microsoft.EntityFrameworkCore;
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

    public async Task<SubForumDto> CreateAsync(CreateSubForumDto request, int userId)
    {
        ValidateFields(request.Name, request.Description);
        await EnsureNameIsAvailableAsync(request.Name);

        User creator = await GetExistingUserAsync(userId);

        SubForum subForum = new()
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            CreatorUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        SubForum created = await subForumRepository.AddAsync(subForum);
        return ToDto(created, creator.UserName);
    }

    public async Task<SubForumDto> UpdateAsync(int id, UpdateSubForumDto request, Caller caller)
    {
        ValidateFields(request.Name, request.Description);

        SubForum existing = await subForumRepository.GetSingleAsync(id);
        Ownership.Ensure(existing.CreatorUserId, caller, "sub-forum");

        await EnsureNameIsAvailableAsync(request.Name, id);

        existing.Name = request.Name.Trim();
        existing.Description = request.Description.Trim();

        await subForumRepository.UpdateAsync(existing);

        return ToDto(existing, await FindUserNameAsync(existing.CreatorUserId));
    }

    public async Task DeleteAsync(int id, Caller caller)
    {
        SubForum existing = await subForumRepository.GetSingleAsync(id);
        Ownership.Ensure(existing.CreatorUserId, caller, "sub-forum");

        List<Post> postsInSubForum = await postRepository.GetMany()
            .Where(post => post.SubForumId == id)
            .ToListAsync();

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
        return ToDto(subForum, await FindUserNameAsync(subForum.CreatorUserId));
    }

    public async Task<IEnumerable<SubForumDto>> GetManyAsync(string? nameContains = null, int? creatorUserId = null)
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

        Dictionary<int, string> userNames = await userRepository.GetMany()
            .Where(user => !user.IsDeleted)
            .ToDictionaryAsync(user => user.Id, user => user.UserName);

        List<SubForum> loadedSubForums = await subForums.ToListAsync();

        return loadedSubForums
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

    private async Task EnsureNameIsAvailableAsync(string name, int? ignoredSubForumId = null)
    {
        string trimmed = name.Trim();

        bool taken = await subForumRepository.GetMany()
            .AnyAsync(subForum => subForum.Id != ignoredSubForumId
                             && subForum.Name.ToLower() == trimmed.ToLower());

        if (taken)
        {
            throw new ArgumentException($"SubForum: '{trimmed}' already exists.");
        }
    }

    private async Task<User> GetExistingUserAsync(int userId)
    {
        User? user = await userRepository.GetMany()
            .FirstOrDefaultAsync(candidate => candidate.Id == userId && !candidate.IsDeleted);

        return user ?? throw new ArgumentException($"User with id: '{userId}' doesn't exist.");
    }

    private async Task<string> FindUserNameAsync(int userId)
    {
        User? user = await userRepository.GetMany()
            .FirstOrDefaultAsync(candidate => candidate.Id == userId && !candidate.IsDeleted);
        return user?.UserName ?? DeletedUser.UserName;
    }

    private static SubForumDto ToDto(SubForum subForum, string creatorUserName) =>
        new(subForum.Id, subForum.Name, subForum.Description, subForum.CreatorUserId, creatorUserName, subForum.CreatedAt);
}
