using CLI.UI.ManagePosts;
using Entities;
using Moq;
using RepositoryContracts;
using Tests.TestHelpers;
using Tests.UnitTests.Fakes;
using Xunit;

namespace Tests.UnitTests.Views;


[Collection(ConsoleCollection.Name)]
public class SinglePostViewTests
{
    private readonly FakePostRepository posts = new();
    private readonly FakeCommentRepository comments = new();
    private readonly SinglePostView view;

    // Runs before every test (xUnit's equivalent of JUnit's @BeforeEach).
    public SinglePostViewTests()
    {
        posts.Seed(NewPost(1), NewPost(2), NewPost(3));
        comments.Seed(
            NewComment(1, postId: 1, "First comment on post 1"),
            NewComment(2, postId: 1, "Last comment on post 1"),
            NewComment(3, postId: 2, "Only comment on post 2"));
        view = new SinglePostView(posts, comments);
    }

    private static Post NewPost(int id) => new()
    {
        Id = id,
        Title = $"Post {id}",
        Body = $"Body {id}",
        UserId = 1,
        CreatedAt = DateTime.Now
    };

    private static Comment NewComment(int id, int postId, string body) => new()
    {
        Id = id,
        PostId = postId,
        UserId = 1,
        Body = body
    };

    [Theory]
    [InlineData(1)] // BVA: lowest existing id
    [InlineData(2)] // EP: representative of the valid partition
    [InlineData(3)] // BVA: highest existing id
    public async Task ShouldShowTheTitle_WhenIdBelongsToAnExistingPost(int id)
    {
        // Arrange
        using var console = new ConsoleSession(id.ToString());
        string expectedTitle = $"Title: Post {id}";

        // Act
        await view.ShowPostAsync();

        // Assert
        Assert.Contains(expectedTitle, console.Output);
    }

    [Fact]
    public async Task ShouldShowTheBody_WhenIdBelongsToAnExistingPost()
    {
        // Arrange
        using var console = new ConsoleSession("2");
        string expectedBody = "Body: Body 2";

        // Act
        await view.ShowPostAsync();

        // Assert
        Assert.Contains(expectedBody, console.Output);
    }

    [Theory]
    [InlineData("0")]   // BVA: just below the lowest existing id
    [InlineData("4")]   // BVA: just above the highest existing id
    [InlineData("-5")]  // EP: negative number
    [InlineData("100")] // EP: large number
    public async Task ShouldSayThePostDoesNotExist_WhenIdHasNoPost(string id)
    {
        // Arrange
        using var console = new ConsoleSession(id);
        string expectedMessage = $"Post with id '{id}' doesn't exist";

        // Act
        await view.ShowPostAsync();

        // Assert
        Assert.Contains(expectedMessage, console.Output);
    }

    // Regression test: an unknown id used to crash the whole application.
    [Fact]
    public async Task ShouldNotThrow_WhenIdHasNoPost()
    {
        // Arrange
        using var console = new ConsoleSession("99");

        // Act
        Exception? exception = await Record.ExceptionAsync(() => view.ShowPostAsync());

        // Assert
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("2147483648")] // BVA: int.MaxValue + 1 - just outside what an int can hold
    [InlineData("abc")]        // EP: letters
    [InlineData("")]           // EP: nothing entered
    [InlineData("1.5")]        // EP: decimal number
    public async Task ShouldSayTheIdIsInvalid_WhenIdIsNotAWholeNumber(string id)
    {
        // Arrange
        using var console = new ConsoleSession(id);
        string expectedMessage = "Invalid ID";

        // Act
        await view.ShowPostAsync();

        // Assert
        Assert.Contains(expectedMessage, console.Output);
    }

    [Fact]
    public async Task ShouldShowNoComments_WhenPostHasNoComments()
    {
        // Arrange - BVA: zero comments
        using var console = new ConsoleSession("3");
        string commentMarker = "- ";

        // Act
        await view.ShowPostAsync();

        // Assert
        Assert.DoesNotContain(commentMarker, console.Output);
    }

    [Fact]
    public async Task ShouldShowTheComment_WhenPostHasOneComment()
    {
        // Arrange - BVA: exactly one comment
        using var console = new ConsoleSession("2");
        string expectedComment = "- Only comment on post 2";

        // Act
        await view.ShowPostAsync();

        // Assert
        Assert.Contains(expectedComment, console.Output);
    }

    [Fact]
    public async Task ShouldShowEveryComment_WhenPostHasSeveralComments()
    {
        // Arrange - BVA (fence-post): both the first and the last comment must be shown
        using var console = new ConsoleSession("1");
        string[] expectedComments = ["- First comment on post 1", "- Last comment on post 1"];

        // Act
        await view.ShowPostAsync();

        // Assert
        Assert.All(expectedComments, comment => Assert.Contains(comment, console.Output));
    }

    [Fact]
    public async Task ShouldNotShowCommentsFromOtherPosts_WhenPostIsShown()
    {
        // Arrange
        using var console = new ConsoleSession("2");
        string commentOnAnotherPost = "comment on post 1";

        // Act
        await view.ShowPostAsync();

        // Assert
        Assert.DoesNotContain(commentOnAnotherPost, console.Output);
    }

    // Mock (Moq) + white-box: this test checks an interaction, not a result. GetSingleAsync throws for
    // an unknown id, and this view avoids that by not calling it at all. The fake cannot tell whether a
    // method was called - a mock can. It is white-box, because a view that called GetSingleAsync and
    // caught the exception would behave the same for the user, but fail this test.
    [Theory]
    [InlineData("4")]   // BVA: just above the highest existing id
    [InlineData("abc")] // EP: not a whole number
    public async Task ShouldNotFetchThePost_WhenIdDoesNotMatchAPost(string id)
    {
        // Arrange
        var postRepository = new Mock<IPostRepository>();
        postRepository.Setup(r => r.GetManyAsync())
            .Returns(new[] { NewPost(1), NewPost(2), NewPost(3) }.AsQueryable());
        var viewWithMock = new SinglePostView(postRepository.Object, comments);
        using var console = new ConsoleSession(id);

        // Act
        await viewWithMock.ShowPostAsync();

        // Assert
        postRepository.Verify(r => r.GetSingleAsync(It.IsAny<int>()), Times.Never);
    }
}
