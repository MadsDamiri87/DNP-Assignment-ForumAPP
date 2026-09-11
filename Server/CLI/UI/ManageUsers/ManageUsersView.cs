using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class ManageUsersView
{
    private CreateUserView createUserView;
    private ListUsersView listUsersView;

    public ManageUsersView(CreateUserView createUserView, ListUsersView listUsersView)
    {
        this.createUserView = createUserView;
        this.listUsersView = listUsersView;
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

            string? input = Console.ReadLine();
            if (input is null)
            {
                running = false;
                continue;
            }

            string choice = input.Trim();

            if (choice == "1")
            {
                await createUserView.CreateUserAsync();
            }
            else if (choice == "2")
            {
                await listUsersView.ShowUsersAsync();
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