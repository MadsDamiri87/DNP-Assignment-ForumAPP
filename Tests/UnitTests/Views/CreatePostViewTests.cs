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

    // Kører før hver test (xUnits svar på JUnits @BeforeEach).
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
    [InlineData("a")]        // BVA: minimumslængde, 1 tegn
    [InlineData("ab")]       // BVA: lige inden for minimum
    [InlineData(ValidTitle)] // EP: repræsentant for den gyldige partition
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
    [InlineData("")]    // BVA: 0 tegn, lige uden for minimum
    [InlineData(" ")]   // BVA: 1 tegn, men kun whitespace
    [InlineData("   ")] // EP: repræsentant for den blanke partition
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
    [InlineData("")]    // BVA: 0 tegn, lige uden for minimum
    [InlineData(" ")]   // BVA: 1 tegn, men kun whitespace
    [InlineData("   ")] // EP: repræsentant for den blanke partition
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
    [InlineData("1")] // BVA: laveste eksisterende bruger-id
    [InlineData("2")] // EP: repræsentant for den gyldige partition
    [InlineData("3")] // BVA: højeste eksisterende bruger-id
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
    [InlineData("0")]          // BVA: lige under laveste eksisterende id
    [InlineData("4")]          // BVA: lige over højeste eksisterende id
    [InlineData("2147483647")] // BVA: int.MaxValue - stadig et gyldigt tal, men ingen sådan bruger
    [InlineData("-5")]         // EP: negativt tal
    [InlineData("100")]        // EP: stort tal
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
    [InlineData("2147483648")] // BVA: int.MaxValue + 1 - lige uden for, hvad en int kan rumme
    [InlineData("abc")]        // EP: bogstaver
    [InlineData("")]           // EP: ingenting indtastet
    [InlineData("1.5")]        // EP: decimaltal
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

    // White-box: lavet ud fra koden, ikke ud fra kravet. Viewet tjekker titel/body, før
    // det parser bruger-id'et, så en blank titel bliver meldt først. Ændres rækkefølgen, går testen i stykker.
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
