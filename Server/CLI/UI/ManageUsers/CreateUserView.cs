using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class CreateUserView
{
    private IUserRepository userRepository;
    
    public CreateUserView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    private bool UserNameExists(string userName)
    {
        bool userExists = userRepository.GetManyAsync().Any(user => user.UserName == userName);
        
        if (!userExists)
        {
            Console.WriteLine($"User with id {userName} not found");
            return false;
        }

        return true;
    }
    
    private bool EmailExists(string email)
    {
        return (userRepository
            .GetManyAsync()
            .Any(user => user.Email == email));
    }
    
    
    public async Task CreateUserAsync()
    {
        Console.WriteLine("Type in new User ID: ");
        string userId = Console.ReadLine()?.Trim() ?? "";

        Console.WriteLine("Type in UserName: ");
        string inputUserName = Console.ReadLine()?.Trim() ?? "";
        
        Console.WriteLine("Type in Email: ");
        string inputEmail = Console.ReadLine()?.Trim() ?? "";
        
        if (UserNameExists(inputUserName))
        {
            Console.WriteLine($"Username: '{inputUserName}' already exists");
            return;
        }
        
        if (EmailExists(inputEmail))
        {
            Console.WriteLine($"Email: '{inputEmail}' already exists");
        }
        
        
        if (!int.TryParse(userId, out int id))
        {
            
        }

        User newUser = new User
        {
            Id = 0,
            UserName = null,
            PasswordHash = null,
            Email = null,
            CreatedDate = default
        };
        

    }
}