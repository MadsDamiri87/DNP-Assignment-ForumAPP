using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class ListUsersView
{
    private readonly IUserRepository userRepository;

    public ListUsersView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public Task ShowUsersAsync()
    {
        Console.WriteLine("List users selected");
        
        IQueryable<User> users = userRepository.GetManyAsync();

        foreach (var user in users)
        {
            Console.WriteLine($"UserName: {user.UserName}");
            Console.WriteLine($"UserID: {user.Id}");
            Console.WriteLine($"Email: {user.Email}");
            Console.WriteLine($"Created: {user.CreatedAt}");
            
        }
        return Task.CompletedTask;
    }
    
    
}