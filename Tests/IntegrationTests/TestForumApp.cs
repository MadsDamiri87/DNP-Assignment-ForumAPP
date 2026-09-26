using CLI.UI;
using CLI.UI.ManagePosts;
using CLI.UI.ManageUsers;
using InMemoryRepositories;
using RepositoryContracts;

namespace Tests.IntegrationTests;

// Bygger hele programmet på samme måde som Program.cs: rigtige in-memory repositories,
// rigtige views, én delt instans af hvert repository. Program.cs bruger top-level statements,
// så dens opsætning kan ikke kaldes fra en test - denne klasse spejler den.
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
