using CLI.UI.ManagePosts;
using CLI.UI.ManageUsers;
using RepositoryContracts;

namespace CLI.UI;

public class CliApp
{
    private readonly ManageUsersView manageUsersView;
    private readonly ManagePostView managePostView;
    
    public CliApp(ManagePostView managePostView, ManageUsersView manageUsersView)
    {
        this.managePostView = managePostView;
        this.manageUsersView = manageUsersView;
    }

    public async Task StartAsync()
    {
        bool running = true;
        
        while (running)
        {
            Console.WriteLine("\n Following options are available: ");
            Console.WriteLine("1. Manage Users ");
            Console.WriteLine("2. Manage Posts ");
            Console.WriteLine("3. Exit \n");
            Console.WriteLine("Chose which one to manage:");
            
            string choice = Console.ReadLine()?.Trim() ?? "";
            
            if  (choice == "1")
            {
                await manageUsersView.StartAsync();
            }
            else if (choice == "2")
            {
                await managePostView.StartAsync();
            }
            else if (choice == "3")
            {
                Console.Write("You have chosen to exit:");
                running = false;
            }
            else
            {
                Console.WriteLine("Invalid choice. Try again.");
            }
            
        }
        
        
    }
}