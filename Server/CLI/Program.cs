using CLI.UI;
using CLI.UI.ManagePosts;
using CLI.UI.ManageUsers;
using InMemoryRepositories;
using RepositoryContracts;

Console.WriteLine("Starting CLI Application...");

ISubForumRepository subForumRepository = new SubForumInMemoryRepository();
IUserRepository userRepository = new UserInMemoryRepository();
ICommentRepository commentRepository = new CommentInMemoryRepository();
IPostRepository postRepository = new PostInMemoryRepository();

DataSeeder temporaryData =
    new DataSeeder(userRepository, postRepository, commentRepository, subForumRepository);
await temporaryData.SeedAsync();


CreatePostView createPostView = new CreatePostView(postRepository, userRepository);
ListPostView listPostView = new ListPostView(postRepository);
SinglePostView singlePostView = new SinglePostView(postRepository, commentRepository);

ManagePostView managePostView = new ManagePostView(createPostView, listPostView, singlePostView);

ListUsersView listUsersView = new ListUsersView(userRepository);
CreateUserView createUserView = new CreateUserView(userRepository);

ManageUsersView manageUsersView = new ManageUsersView(createUserView, listUsersView);


CliApp cliApp = new CliApp(managePostView, manageUsersView);

await cliApp.StartAsync();
