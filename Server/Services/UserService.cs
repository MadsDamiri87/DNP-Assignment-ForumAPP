using ApiContracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using ServiceContracts;

namespace Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;

    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<UserDto> CreateAsync(CreateUserDto request)
    {
        ValidateFields(request.UserName, request.Password, request.Email);
        await EnsureUserNameIsAvailableAsync(request.UserName);
        await EnsureEmailIsAvailableAsync(request.Email);

        User user = new()
        {
            UserName = request.UserName.Trim(),
            Password = request.Password,
            Email = request.Email.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        User created = await userRepository.AddAsync(user);
        return ToDto(created);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserDto request, Caller caller)
    {
        ValidateFields(request.UserName, request.Password, request.Email);

        User existing = await userRepository.GetSingleAsync(id);
        Ownership.Ensure(existing.Id, caller, "account");

        await EnsureUserNameIsAvailableAsync(request.UserName, id);
        await EnsureEmailIsAvailableAsync(request.Email, id);

        existing.UserName = request.UserName.Trim();
        existing.Password = request.Password;
        existing.Email = request.Email.Trim();


        await userRepository.UpdateAsync(existing);
        return ToDto(existing);
    }

    public async Task DeleteAsync(int id, Caller caller)
    {
        User existing = await userRepository.GetSingleAsync(id);
        Ownership.Ensure(existing.Id, caller, "account");

        await userRepository.DeleteAsync(id);
    }

    public async Task<UserDto> GetSingleAsync(int id)
    {
        User user = await userRepository.GetSingleAsync(id);
        return ToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetManyAsync(string? userNameContains = null)
    {
        IQueryable<User> users = userRepository.GetMany();

        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            string filter = userNameContains.Trim().ToLower();

            users = users.Where(user => user.UserName.ToLower().Contains(filter));
        }

        List<User> loadUsers = await users.ToListAsync();

        return loadUsers.Select(ToDto).ToList();
    }

    private static void ValidateFields(string userName, string password, string email)
    {
        if (string.IsNullOrWhiteSpace(userName)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("UserName, password and email are required.");
        }
    }

    private async Task EnsureUserNameIsAvailableAsync(string userName, int? ignoredUserId = null)
    {
        bool taken = await userRepository.GetMany()
            .AnyAsync(user => user.Id != ignoredUserId
                         && user.UserName.ToLower() == userName.Trim().ToLower());

        if (taken)
        {
            throw new ArgumentException($"Username: '{userName.Trim()}' already exists.");
        }
    }

    private async Task EnsureEmailIsAvailableAsync(string email, int? ignoredUserId = null)
    {
        bool taken = await userRepository.GetMany()
            .AnyAsync(user => user.Id != ignoredUserId
                         && user.Email.ToLower() == email.Trim().ToLower());

        if (taken)
        {
            throw new ArgumentException($"Email: '{email.Trim()}' already exists.");
        }
    }

    private static UserDto ToDto(User user) =>
        new(user.Id, user.UserName, user.Email, user.Role, user.CreatedAt);
}
