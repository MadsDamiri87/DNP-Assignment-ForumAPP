using CLI.UI.ManagePosts;
using Entities;
using Tests.TestHelpers;
using Tests.UnitTests.Fakes;
using Xunit;

namespace Tests.UnitTests.Views;


[Collection(ConsoleCollection.Name)]
public class CreatePostViewTests
{
    private const string ValidTitle = "A normal title";
    private const string ValidBody = "A normal body";
    private const int ExistingUserId = 2;

    private readonly FakePostRepository posts = new();
    private readonly FakeUserRepository users = new();
    private readonly CreatePostView view;

    // Runs before every test (xUnit's equivalent of JUnit's @BeforeEach).
    public CreatePostViewTests()
    {
        users.Seed(NewUser(1), NewUser(2), NewUser(3));
        view = new CreatePostView(posts, users);
    }

    private static User NewUser(int id) => new()
    {
        Id = id,
        UserName = $"user{id}",
        PasswordHash = "hash",
        Email = $"user{id}@x.dk"
    };

    [Fact]
    public async Task ShouldAddOnePost_WhenAllInputIsValid()
    {
        // Arrange
        using var console = new ConsoleSession(ValidTitle, ValidBody, ExistingUserId.ToString());

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Single(posts.Added);
    }

    [Fact]
    public async Task ShouldStoreTheEnteredValues_WhenAllInputIsValid()
    {
        // Arrange
        using var console = new ConsoleSession(ValidTitle, ValidBody, ExistingUserId.ToString());
        var expected = (ValidTitle, ValidBody, ExistingUserId);

        // Act
        await view.CreatePostAsync();

        // Assert
        Post added = posts.Added.Single();
        Assert.Equal(expected, (added.Title, added.Body, added.UserId));
    }

    [Fact]
    public async Task ShouldPrintTheNewPostId_WhenPostIsCreated()
    {
        // Arrange
        using var console = new ConsoleSession(ValidTitle, ValidBody, ExistingUserId.ToString());
        string expectedMessage = "Post created with id: 1";

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Contains(expectedMessage, console.Output);
    }

    [Fact]
    public async Task ShouldTrimTitleAndBody_WhenInputHasSurroundingSpaces()
    {
        // Arrange
        using var console = new ConsoleSession($"  {ValidTitle}  ", $"  {ValidBody}  ", ExistingUserId.ToString());
        var expected = (ValidTitle, ValidBody);

        // Act
        await view.CreatePostAsync();

        // Assert
        Post added = posts.Added.Single();
        Assert.Equal(expected, (added.Title, added.Body));
    }

    [Theory]
    [InlineData("a")]        // BVA: minimum length, 1 character
    [InlineData("ab")]       // BVA: just inside the minimum
    [InlineData(ValidTitle)] // EP: representative of the valid partition
    public async Task ShouldAddPost_WhenTitleHasAtLeastOneCharacter(string title)
    {
        // Arrange
        using var console = new ConsoleSession(title, ValidBody, ExistingUserId.ToString());

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Single(posts.Added);
    }

    [Theory]
    [InlineData("")]    // BVA: 0 characters, just outside the minimum
    [InlineData(" ")]   // BVA: 1 character, but only whitespace
    [InlineData("   ")] // EP: representative of the blank partition
    public async Task ShouldNotAddPost_WhenTitleIsBlank(string title)
    {
        // Arrange
        using var console = new ConsoleSession(title, ValidBody, ExistingUserId.ToString());

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Empty(posts.Added);
    }

    [Theory]
    [InlineData("")]    // BVA: 0 characters, just outside the minimum
    [InlineData(" ")]   // BVA: 1 character, but only whitespace
    [InlineData("   ")] // EP: representative of the blank partition
    public async Task ShouldNotAddPost_WhenBodyIsBlank(string body)
    {
        // Arrange
        using var console = new ConsoleSession(ValidTitle, body, ExistingUserId.ToString());

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Empty(posts.Added);
    }

    [Fact]
    public async Task ShouldExplainThatTitleAndBodyAreRequired_WhenTitleIsBlank()
    {
        // Arrange
        using var console = new ConsoleSession("", ValidBody, ExistingUserId.ToString());
        string expectedMessage = "Title and body are required";

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Contains(expectedMessage, console.Output);
    }

    [Theory]
    [InlineData("1")] // BVA: lowest existing user id
    [InlineData("2")] // EP: representative of the valid partition
    [InlineData("3")] // BVA: highest existing user id
    public async Task ShouldAddPost_WhenUserIdBelongsToAnExistingUser(string userId)
    {
        // Arrange
        using var console = new ConsoleSession(ValidTitle, ValidBody, userId);

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Single(posts.Added);
    }

    [Theory]
    [InlineData("0")]          // BVA: just below the lowest existing id
    [InlineData("4")]          // BVA: just above the highest existing id
    [InlineData("2147483647")] // BVA: int.MaxValue - still a valid number, but no such user
    [InlineData("-5")]         // EP: negative number
    [InlineData("100")]        // EP: large number
    public async Task ShouldNotAddPost_WhenUserIdHasNoUser(string userId)
    {
        // Arrange
        using var console = new ConsoleSession(ValidTitle, ValidBody, userId);

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Empty(posts.Added);
    }

    [Fact]
    public async Task ShouldExplainThatTheUserDoesNotExist_WhenUserIdHasNoUser()
    {
        // Arrange
        using var console = new ConsoleSession(ValidTitle, ValidBody, "4");
        string expectedMessage = "User with id '4' doesn't exist";

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Contains(expectedMessage, console.Output);
    }

    [Theory]
    [InlineData("2147483648")] // BVA: int.MaxValue + 1 - just outside what an int can hold
    [InlineData("abc")]        // EP: letters
    [InlineData("")]           // EP: nothing entered
    [InlineData("1.5")]        // EP: decimal number
    public async Task ShouldNotAddPost_WhenUserIdIsNotAWholeNumber(string userId)
    {
        // Arrange
        using var console = new ConsoleSession(ValidTitle, ValidBody, userId);

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Empty(posts.Added);
    }

    [Fact]
    public async Task ShouldExplainThatTheUserIdIsInvalid_WhenUserIdIsNotAWholeNumber()
    {
        // Arrange
        using var console = new ConsoleSession(ValidTitle, ValidBody, "abc");
        string expectedMessage = "Invalid User ID";

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Contains(expectedMessage, console.Output);
    }

    // White-box: designed from the code, not from the requirement. The view checks title/body before
    // it parses the user id, so a blank title is reported first. Reordering the checks breaks this test.
    [Fact]
    public async Task ShouldReportBlankTitleFirst_WhenTitleIsBlankAndUserIdIsInvalid()
    {
        // Arrange
        using var console = new ConsoleSession("", ValidBody, "abc");
        string expectedMessage = "Title and body are required";

        // Act
        await view.CreatePostAsync();

        // Assert
        Assert.Contains(expectedMessage, console.Output);
    }
}
