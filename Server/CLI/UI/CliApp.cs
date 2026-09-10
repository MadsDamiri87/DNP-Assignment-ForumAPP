using CLI.UI.ManagePosts;
using CLI.UI.ManageUsers;
using RepositoryContracts;

namespace CLI.UI;

public class CliApp
{
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;
    private readonly IPostRepository postRepository;

    private readonly ManageUsersView manageUsersView;
    private readonly ManagePostView managePostView;
    
    
    
    public CliApp(
        IUserRepository userRepository, 
        ICommentRepository commentRepository, 
        IPostRepository postRepository)
    {
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
    }

    public async Task StartAsync()
    {
        bool running = true;
        
        while (running)
        {
            Console.Write("Following options are available: ");
            Console.Write("1. Manage Users ");
            Console.Write("2. Manage Posts ");
            Console.Write("3. Exit \n");
            Console.Write("Chose which one to manage:");
            
            string choice = Console.ReadLine()?.Trim() ?? "";
            
            if  (choice == "1")
            {
                
            }
            else if (choice == "2")
            {
                
            }
            else if (choice == "3")
            {
                Console.Write("You have chosen to exit:");
                running = false;
            }
            else
            {
                Console.WriteLine("Invalid choice. Try again.");
                StartAsync();
            }
            
        }
        
        
    }
}