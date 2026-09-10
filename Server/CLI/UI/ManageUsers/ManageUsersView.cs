using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class ManageUsersView
{
    private readonly IUserRepository _userRepository;

    public ManageUsersView(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task StartAsync()
    {
        bool running = true;

        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("Manage Users");
            Console.WriteLine("1. Create user");
            Console.WriteLine("2. List users");
            Console.WriteLine("3. Back");
            Console.Write("Choose an option: ");

            string choice = Console.ReadLine()?.Trim() ?? "";

            if (choice == "1")
            {
                Console.WriteLine("Create user selected");
            }
            else if (choice == "2")
            {
                Console.WriteLine("List users selected");
            }
            else if (choice == "3")
            {
                running = false;
            }
            else
            {
                Console.WriteLine("Invalid choice");
            }
        }
    }
}