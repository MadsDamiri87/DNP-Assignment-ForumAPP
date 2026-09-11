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
        Console.WriteLine("Create user selected");

        Console.WriteLine("Type in UserName: ");
        string inputUserName = Console.ReadLine()?.Trim() ?? "";
        
        Console.WriteLine("Type in Password: ");
        string inputPassword = Console.ReadLine()?.Trim() ?? "";

        Console.WriteLine("Type in Email: ");
        string inputEmail = Console.ReadLine()?.Trim() ?? "";
        
        if (string.IsNullOrWhiteSpace(inputUserName)
            || string.IsNullOrWhiteSpace(inputPassword)
            || string.IsNullOrWhiteSpace(inputEmail))
        {
            Console.WriteLine("UserName, password and email are required");
            return;
        }

        if (UserNameExists(inputUserName))
        {
            Console.WriteLine($"Username: '{inputUserName}' already exists");
            return;
        }
        
        if (EmailExists(inputEmail))
        {
            Console.WriteLine($"Email: '{inputEmail}' already exists");
            return;
        }
        

        User newUser = new User
        {
            UserName = inputUserName,
            PasswordHash = inputPassword,
            Email = inputEmail,
            CreatedDate = DateTime.Now
        };
        
        User createdUser = await userRepository.AddAsync(newUser);
        
        Console.WriteLine($"User created with id: {createdUser.Id}");
    }
}