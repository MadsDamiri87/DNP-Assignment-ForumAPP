using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ManagePostView
{
    private readonly IPostRepository postRepository;

    public ManagePostView(IPostRepository postRepository)
    {
        this.postRepository = postRepository;
    }

    public async Task StartAsync()
    {
        bool running = true;

        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("Manage Posts");
            Console.WriteLine("1. Create Post");
            Console.WriteLine("2. List Post");
            Console.WriteLine("3. Back");
            Console.Write("Choose an option: ");

            string choice = Console.ReadLine()?.Trim() ?? "";

            if (choice == "1")
            {
                Console.WriteLine("Create post selected");
            }
            else if (choice == "2")
            {
                Console.WriteLine("List post selected");
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