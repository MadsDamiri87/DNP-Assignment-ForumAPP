using CLI.UI.ManagePosts;
using Entities;
using Tests.TestHelpers;
using Tests.UnitTests.Fakes;
using Xunit;

namespace Tests.UnitTests.Views;

[Collection(ConsoleCollection.Name)]
public class AddCommentViewTests
{
    private const string ValidBody = "A normal comment";
    private const int ExistingPostId = 1;
    private const int ExistingUserId = 2;

    private readonly FakeCommentRepository comments = new();
    private readonly FakePostRepository posts = new();
    private readonly FakeUserRepository users = new();
    private readonly AddCommentView view;

    // Kører før hver test (xUnits svar på JUnits @BeforeEach).
    public AddCommentViewTests()
    {
        users.Seed(NewUser(ExistingUserId));
        posts.Seed(NewPost(ExistingPostId));
        view = new AddCommentView(comments, posts, users);
    }

    private static User NewUser(int id) => new()
    {
        Id = id,
        UserName = $"user{id}",
        Password = "hash",
        Email = $"user{id}@x.dk"
    };

    private static Post NewPost(int id) => new()
    {
        Id = id,
        Title = "A normal title",
        Body = "A normal body",
        UserId = ExistingUserId,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task ShouldAddOneComment_WhenAllInputIsValid()
    {
        // Arrange
        using var console = new ConsoleSession(
            ExistingPostId.ToString(), ExistingUserId.ToString(), ValidBody);

        // Act
        await view.AddCommentAsync();

        // Assert
        Assert.Single(comments.Added);
    }

    [Fact]
    public async Task ShouldStoreTheEnteredValues_WhenAllInputIsValid()
    {
        // Arrange
        using var console = new ConsoleSession(
            ExistingPostId.ToString(), ExistingUserId.ToString(), ValidBody);
        var expected = (ExistingPostId, ExistingUserId, ValidBody);

        // Act
        await view.AddCommentAsync();

        // Assert
        Comment added = comments.Added.Single();
        Assert.Equal(expected, (added.PostId, added.UserId, added.Body));
    }

    [Fact]
    public async Task ShouldNotAddAnything_WhenThePostDoesNotExist()
    {
        // Arrange - post 99 er ikke seeded.
        using var console = new ConsoleSession(
            "99", ExistingUserId.ToString(), ValidBody);

        // Act
        await view.AddCommentAsync();

        // Assert
        Assert.Empty(comments.Added);
    }

    [Fact]
    public async Task ShouldNotAddAnything_WhenTheUserDoesNotExist()
    {
        // Arrange - bruger 99 er ikke seeded.
        using var console = new ConsoleSession(
            ExistingPostId.ToString(), "99", ValidBody);

        // Act
        await view.AddCommentAsync();

        // Assert
        Assert.Empty(comments.Added);
    }

    [Fact]
    public async Task ShouldNotAddAnything_WhenTheBodyIsEmpty()
    {
        // Arrange
        using var console = new ConsoleSession(
            ExistingPostId.ToString(), ExistingUserId.ToString(), "");

        // Act
        await view.AddCommentAsync();

        // Assert
        Assert.Empty(comments.Added);
    }

    [Fact]
    public async Task ShouldNotAddAnything_WhenThePostIdIsNotANumber()
    {
        // Arrange
        using var console = new ConsoleSession(
            "abc", ExistingUserId.ToString(), ValidBody);

        // Act
        await view.AddCommentAsync();

        // Assert
        Assert.Empty(comments.Added);
    }
}
