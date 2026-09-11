using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ManagePostView
{
    private CreatePostView createPostView;
    private ListPostView listPostView;
    private SinglePostView singlePostView;

    public ManagePostView(CreatePostView createPostView,  ListPostView listPostView, SinglePostView singlePostView)
    {
        this.createPostView = createPostView;
        this.listPostView = listPostView;
        this.singlePostView = singlePostView;
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
            Console.WriteLine("3. View Post");
            Console.WriteLine("4. Back");
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
                await createPostView.CreatePostAsync();
                
            }
            else if (choice == "2")
            {
                await listPostView.ShowPostsAsync();
            }
            
            else if (choice == "3")
            {
                await singlePostView.ShowPostAsync();
            }
            
            else if (choice == "4")
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