using CLI.UI.ManageUsers;
using Entities;
using Tests.TestHelpers;
using Tests.UnitTests.Fakes;
using Xunit;

namespace Tests.UnitTests.Views;


[Collection(ConsoleCollection.Name)]
public class CreateUserViewTests
{
    private const string ValidUserName = "mads";
    private const string ValidPassword = "secret";
    private const string ValidEmail = "mads@x.dk";
    private const string TakenUserName = "user1";
    private const string TakenEmail = "user1@x.dk";

    private readonly FakeUserRepository users = new();
    private readonly CreateUserView view;

    // Runs before every test (xUnit's equivalent of JUnit's @BeforeEach).
    public CreateUserViewTests()
    {
        users.Seed(new User { Id = 1, UserName = TakenUserName, PasswordHash = "hash", Email = TakenEmail });
        view = new CreateUserView(users);
    }

    // JUnit: @MethodSource. Each row is one test run.
    public static TheoryData<string, string, string> BlankFieldCases => new()
    {
        { "", ValidPassword, ValidEmail },     // BVA: user name with 0 characters
        { " ", ValidPassword, ValidEmail },    // BVA: user name with 1 whitespace character
        { ValidUserName, "", ValidEmail },     // BVA: password with 0 characters
        { ValidUserName, " ", ValidEmail },    // BVA: password with 1 whitespace character
        { ValidUserName, ValidPassword, "" },  // BVA: email with 0 characters
        { ValidUserName, ValidPassword, " " }, // BVA: email with 1 whitespace character
    };

    public static TheoryData<string, string, string, string> RejectionCases => new()
    {
        { "", ValidPassword, ValidEmail, "UserName, password and email are required" },
        { TakenUserName, ValidPassword, ValidEmail, $"Username: '{TakenUserName}' already exists" },
        { ValidUserName, ValidPassword, TakenEmail, $"Email: '{TakenEmail}' already exists" },
    };

    [Fact]
    public async Task ShouldAddOneUser_WhenAllInputIsValid()
    {
        // Arrange
        using var console = new ConsoleSession(ValidUserName, ValidPassword, ValidEmail);

        // Act
        await view.CreateUserAsync();

        // Assert
        Assert.Single(users.Added);
    }

    [Fact]
    public async Task ShouldStoreTheEnteredValues_WhenAllInputIsValid()
    {
        // Arrange
        using var console = new ConsoleSession(ValidUserName, ValidPassword, ValidEmail);
        var expected = (ValidUserName, ValidPassword, ValidEmail);

        // Act
        await view.CreateUserAsync();

        // Assert
        User added = users.Added.Single();
        Assert.Equal(expected, (added.UserName, added.PasswordHash, added.Email));
    }

    [Fact]
    public async Task ShouldPrintTheNewUserId_WhenUserIsCreated()
    {
        // Arrange
        using var console = new ConsoleSession(ValidUserName, ValidPassword, ValidEmail);
        string expectedMessage = "User created with id: 2";

        // Act
        await view.CreateUserAsync();

        // Assert
        Assert.Contains(expectedMessage, console.Output);
    }

    [Fact]
    public async Task ShouldSetCreatedAtToNow_WhenUserIsCreated()
    {
        // Arrange
        using var console = new ConsoleSession(ValidUserName, ValidPassword, ValidEmail);
        DateTime before = DateTime.Now;

        // Act
        await view.CreateUserAsync();

        // Assert
        User added = users.Added.Single();
        Assert.InRange(added.CreatedAt, before, DateTime.Now);
    }

    [Fact]
    public async Task ShouldAddUser_WhenEveryFieldIsOneCharacterLong()
    {
        // Arrange - BVA: every field at its minimum length
        using var console = new ConsoleSession("a", "b", "c");

        // Act
        await view.CreateUserAsync();

        // Assert
        Assert.Single(users.Added);
    }

    [Theory]
    [MemberData(nameof(BlankFieldCases))]
    public async Task ShouldNotAddUser_WhenAFieldIsBlank(string userName, string password, string email)
    {
        // Arrange
        using var console = new ConsoleSession(userName, password, email);

        // Act
        await view.CreateUserAsync();

        // Assert
        Assert.Empty(users.Added);
    }

    [Fact]
    public async Task ShouldNotAddUser_WhenUserNameIsTaken()
    {
        // Arrange
        using var console = new ConsoleSession(TakenUserName, ValidPassword, ValidEmail);

        // Act
        await view.CreateUserAsync();

        // Assert
        Assert.Empty(users.Added);
    }

    [Fact]
    public async Task ShouldNotAddUser_WhenEmailIsTaken()
    {
        // Arrange
        using var console = new ConsoleSession(ValidUserName, ValidPassword, TakenEmail);

        // Act
        await view.CreateUserAsync();

        // Assert
        Assert.Empty(users.Added);
    }

    [Theory]
    [MemberData(nameof(RejectionCases))]
    public async Task ShouldExplainWhy_WhenUserIsRejected(
        string userName, string password, string email, string expectedMessage)
    {
        // Arrange
        using var console = new ConsoleSession(userName, password, email);

        // Act
        await view.CreateUserAsync();

        // Assert
        Assert.Contains(expectedMessage, console.Output);
    }

    // White-box: designed from the code. The view checks the user name before the email, so when both
    // are taken, the user name is reported. Reordering the checks breaks this test.
    [Fact]
    public async Task ShouldReportTakenUserNameFirst_WhenBothUserNameAndEmailAreTaken()
    {
        // Arrange
        using var console = new ConsoleSession(TakenUserName, ValidPassword, TakenEmail);
        string expectedMessage = $"Username: '{TakenUserName}' already exists";

        // Act
        await view.CreateUserAsync();

        // Assert
        Assert.Contains(expectedMessage, console.Output);
    }
}
