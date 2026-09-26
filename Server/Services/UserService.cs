using ApiContracts;
using Entities;
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
        EnsureUserNameIsAvailable(request.UserName);
        EnsureEmailIsAvailable(request.Email);

        User user = new()
        {
            UserName = request.UserName.Trim(),
            PasswordHash = request.Password,
            Email = request.Email.Trim(),
            CreatedAt = DateTime.Now
        };

        User created = await userRepository.AddAsync(user);
        return ToDto(created);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserDto request)
    {
        ValidateFields(request.UserName, request.Password, request.Email);

        User existing = await userRepository.GetSingleAsync(id);

        EnsureUserNameIsAvailable(request.UserName, id);
        EnsureEmailIsAvailable(request.Email, id);

        User updated = new()
        {
            Id = existing.Id,
            UserName = request.UserName.Trim(),
            PasswordHash = request.Password,
            Email = request.Email.Trim(),
            CreatedAt = existing.CreatedAt
        };

        await userRepository.UpdateAsync(updated);
        return ToDto(updated);
    }

    public Task DeleteAsync(int id)
    {
        return userRepository.DeleteAsync(id);
    }

    public async Task<UserDto> GetSingleAsync(int id)
    {
        User user = await userRepository.GetSingleAsync(id);
        return ToDto(user);
    }

    public IEnumerable<UserDto> GetMany(string? userNameContains = null)
    {
        IQueryable<User> users = userRepository.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            string filter = userNameContains.Trim();
            users = users.Where(user => user.UserName.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        return users.Select(ToDto).ToList();
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

    private void EnsureUserNameIsAvailable(string userName, int? ignoredUserId = null)
    {
        bool taken = userRepository.GetManyAsync()
            .Any(user => user.Id != ignoredUserId && user.UserName == userName.Trim());

        if (taken)
        {
            throw new ArgumentException($"Username: '{userName.Trim()}' already exists.");
        }
    }

    private void EnsureEmailIsAvailable(string email, int? ignoredUserId = null)
    {
        bool taken = userRepository.GetManyAsync()
            .Any(user => user.Id != ignoredUserId && user.Email == email.Trim());

        if (taken)
        {
            throw new ArgumentException($"Email: '{email.Trim()}' already exists.");
        }
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        CreatedAt = user.CreatedAt
    };
}
