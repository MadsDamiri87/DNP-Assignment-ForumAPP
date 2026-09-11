using CLI.UI.ManagePosts;
using Entities;
using Tests.TestHelpers;
using Tests.UnitTests.Fakes;
using Xunit;

namespace Tests.UnitTests.Views;


[Collection(ConsoleCollection.Name)]
public class ListPostViewTests
{
    private readonly FakePostRepository posts = new();
    private readonly ListPostView view;

    // Runs before every test (xUnit's equivalent of JUnit's @BeforeEach).
    public ListPostViewTests()
    {
        view = new ListPostView(posts);
    }

    private static Post NewPost(int id, string body = "Body") => new()
    {
        Id = id,
        Title = $"Post {id}",
        Body = body,
        UserId = 1,
        CreatedAt = DateTime.Now
    };

    [Theory]
    [InlineData(0)] // BVA: no posts
    [InlineData(1)] // BVA: exactly one post
    [InlineData(3)] // EP: representative of "several posts"
    public async Task ShouldShowOneEntryPerPost_WhenRepositoryHasThatManyPosts(int count)
    {
        // Arrange
        posts.Seed(Enumerable.Range(1, count).Select(id => NewPost(id)).ToArray());
        using var console = new ConsoleSession();

        // Act
        await view.ShowPostsAsync();

        // Assert
        int shownEntries = console.Output.Split("ID: ").Length - 1;
        Assert.Equal(count, shownEntries);
    }

    [Fact]
    public async Task ShouldShowTheIdFollowedByTheTitle_WhenPostIsListed()
    {
        // Arrange
        posts.Seed(NewPost(1));
        using var console = new ConsoleSession();
        string expectedEntry = $"ID: 1{Environment.NewLine}Title: Post 1";

        // Act
        await view.ShowPostsAsync();

        // Assert
        Assert.Contains(expectedEntry, console.Output);
    }

    [Fact]
    public async Task ShouldNotShowTheBody_WhenPostsAreListed()
    {
        // Arrange
        string body = "Body text that belongs on the single post page";
        posts.Seed(NewPost(1, body));
        using var console = new ConsoleSession();

        // Act
        await view.ShowPostsAsync();

        // Assert
        Assert.DoesNotContain(body, console.Output);
    }
}
