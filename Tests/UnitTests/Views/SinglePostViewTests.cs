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

    // Kører før hver test (xUnits svar på JUnits @BeforeEach).
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
    [InlineData(1)] // BVA: laveste eksisterende id
    [InlineData(2)] // EP: repræsentant for den gyldige partition
    [InlineData(3)] // BVA: højeste eksisterende id
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
    [InlineData("0")]   // BVA: lige under laveste eksisterende id
    [InlineData("4")]   // BVA: lige over højeste eksisterende id
    [InlineData("-5")]  // EP: negativt tal
    [InlineData("100")] // EP: stort tal
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

    // Regressionstest: et ukendt id plejede at crashe hele programmet - det må ikke ske igen.
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
    [InlineData("2147483648")] // BVA: int.MaxValue + 1 - lige uden for, hvad en int kan rumme
    [InlineData("abc")]        // EP: bogstaver
    [InlineData("")]           // EP: ingenting indtastet
    [InlineData("1.5")]        // EP: decimaltal
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
        // Arrange - BVA: nul kommentarer
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
        // Arrange - BVA: præcis én kommentar
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
        // Arrange - BVA (fence-post): både den første og den sidste kommentar skal vises
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

    // Mock (Moq) + white-box: testen tjekker en interaktion, ikke et resultat. GetSingleAsync kaster ved
    // et ukendt id, og viewet undgår det ved slet ikke at kalde den. Fake'n kan ikke se, om en
    // metode blev kaldt - en mock kan. Det er white-box, fordi et view, der kaldte GetSingleAsync og
    // fangede exception'en, ville opføre sig ens for brugeren, men alligevel fejle testen.
    [Theory]
    [InlineData("4")]   // BVA: lige over højeste eksisterende id
    [InlineData("abc")] // EP: ikke et helt tal
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
