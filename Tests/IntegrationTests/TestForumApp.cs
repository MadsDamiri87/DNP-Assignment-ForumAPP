using CLI.UI;
using CLI.UI.ManagePosts;
using CLI.UI.ManageUsers;
using InMemoryRepositories;
using RepositoryContracts;

namespace Tests.IntegrationTests;

// Builds the whole application the same way Program.cs does: real in-memory repositories,
// real views, one shared instance of each repository. Program.cs uses top-level statements,
// so its wiring cannot be called from a test - this class mirrors it.
public sealed class TestForumApp
{
    public IUserRepository Users { get; } = new UserInMemoryRepository();
    public IPostRepository Posts { get; } = new PostInMemoryRepository();
    public ICommentRepository Comments { get; } = new CommentInMemoryRepository();
    public ISubForumRepository SubForums { get; } = new SubForumInMemoryRepository();

    public CliApp App { get; }

    private TestForumApp()
    {
        var createPostView = new CreatePostView(Posts, Users);
        var listPostView = new ListPostView(Posts);
        var singlePostView = new SinglePostView(Posts, Comments);
        var managePostView = new ManagePostView(createPostView, listPostView, singlePostView);

        var listUsersView = new ListUsersView(Users);
        var createUserView = new CreateUserView(Users);
        var manageUsersView = new ManageUsersView(createUserView, listUsersView);

        App = new CliApp(managePostView, manageUsersView);
    }

    public static async Task<TestForumApp> CreateSeededAsync()
    {
        var forum = new TestForumApp();
        await new DataSeeder(forum.Users, forum.Posts, forum.Comments, forum.SubForums).SeedAsync();
        return forum;
    }
}
