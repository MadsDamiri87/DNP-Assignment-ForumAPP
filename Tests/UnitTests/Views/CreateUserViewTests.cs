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

    // Kører før hver test (xUnits svar på JUnits @BeforeEach).
    public CreateUserViewTests()
    {
        users.Seed(new User { Id = 1, UserName = TakenUserName, PasswordHash = "hash", Email = TakenEmail });
        view = new CreateUserView(users);
    }

    // JUnit: @MethodSource. Hver række er én testkørsel.
    public static TheoryData<string, string, string> BlankFieldCases => new()
    {
        { "", ValidPassword, ValidEmail },     // BVA: brugernavn med 0 tegn
        { " ", ValidPassword, ValidEmail },    // BVA: brugernavn med 1 whitespace-tegn
        { ValidUserName, "", ValidEmail },     // BVA: password med 0 tegn
        { ValidUserName, " ", ValidEmail },    // BVA: password med 1 whitespace-tegn
        { ValidUserName, ValidPassword, "" },  // BVA: email med 0 tegn
        { ValidUserName, ValidPassword, " " }, // BVA: email med 1 whitespace-tegn
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
        // Arrange - BVA: alle felter på minimumslængde
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

    // White-box: lavet ud fra koden. Viewet tjekker brugernavn før email, så når begge
    // er optaget, er det brugernavnet, der meldes. Ændres rækkefølgen, går testen i stykker.
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
