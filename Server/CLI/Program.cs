using CLI.UI;
using CLI.UI.ManagePosts;
using CLI.UI.ManageUsers;
using FileRepository;
using RepositoryContracts;

Console.WriteLine("Starting CLI Application...");

ISubForumRepository subForumRepository = new SubForumFileRepository();
IUserRepository userRepository = new UserFileRepository();
ICommentRepository commentRepository = new CommentFileRepository();
IPostRepository postRepository = new PostFileRepository();

CreatePostView createPostView = new CreatePostView(postRepository, userRepository);
ListPostView listPostView = new ListPostView(postRepository);
SinglePostView singlePostView = new SinglePostView(postRepository, commentRepository);

ManagePostView managePostView = new ManagePostView(createPostView, listPostView, singlePostView);

ListUsersView listUsersView = new ListUsersView(userRepository);
CreateUserView createUserView = new CreateUserView(userRepository);

ManageUsersView manageUsersView = new ManageUsersView(createUserView, listUsersView);


CliApp cliApp = new CliApp(managePostView, manageUsersView);

await cliApp.StartAsync();
