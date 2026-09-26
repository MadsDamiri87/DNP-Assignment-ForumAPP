using ApiContracts;
using Entities;
using Services;
using Tests.UnitTests.Fakes;
using Xunit;

namespace Tests.UnitTests.Services;

// Unit tests af UserService med et fake repository, så kun servicens egen logik testes.
// Black-box: designet ud fra forretningsreglerne, ikke ud fra implementeringen.
// Testnavne: Should<Resultat>_When<Betingelse>.
//
// UserName, Password og Email - påkrævet tekst:
//   Partition (EP)          | Repræsentant | BVA-værdier           | Forventet
//   blank                   | "   "        | "" (0 tegn), " " (1)  | afvises
//   mindst ét tegn          | "mads"       | "a" (1 tegn)          | accepteres
//
// Entydighed - der findes en bruger med "user1" og "user1@x.dk":
//   brugernavn optaget      | "user1"      | afvises
//   email optaget           | "user1@x.dk" | afvises
//   begge frie              | "mads"       | accepteres
public class UserServiceTests
{
    private const string ValidUserName = "mads";
    private const string ValidPassword = "secret";
    private const string ValidEmail = "mads@x.dk";
    private const string TakenUserName = "user1";
    private const string TakenEmail = "user1@x.dk";

    private readonly FakeUserRepository users = new();
    private readonly UserService service;

    // Kører før hver test (xUnits svar på JUnits @BeforeEach).
    public UserServiceTests()
    {
        users.Seed(new User
        {
            Id = 1,
            UserName = TakenUserName,
            PasswordHash = "hash",
            Email = TakenEmail,
            CreatedAt = new DateTime(2026, 1, 1)
        });
        service = new UserService(users);
    }

    private static CreateUserDto NewRequest(
        string userName = ValidUserName, string password = ValidPassword, string email = ValidEmail) =>
        new() { UserName = userName, Password = password, Email = email };

    public static TheoryData<string, string, string> BlankeFelter => new()
    {
        { "", ValidPassword, ValidEmail },
        { " ", ValidPassword, ValidEmail },
        { ValidUserName, "", ValidEmail },
        { ValidUserName, " ", ValidEmail },
        { ValidUserName, ValidPassword, "" },
        { ValidUserName, ValidPassword, " " },
    };

    [Fact]
    public async Task ShouldStoreTheUser_WhenInputIsValid()
    {
        // Arrange
        CreateUserDto request = NewRequest();

        // Act
        await service.CreateAsync(request);

        // Assert
        User added = Assert.Single(users.Added);
        Assert.Equal((ValidUserName, ValidPassword, ValidEmail), (added.UserName, added.PasswordHash, added.Email));
    }

    [Fact]
    public async Task ShouldReturnTheNewId_WhenUserIsCreated()
    {
        // Arrange
        int expectedId = 2;

        // Act
        UserDto created = await service.CreateAsync(NewRequest());

        // Assert
        Assert.Equal(expectedId, created.Id);
    }

    [Fact]
    public async Task ShouldSetCreatedAtToNow_WhenUserIsCreated()
    {
        // Arrange
        DateTime før = DateTime.Now;

        // Act
        UserDto created = await service.CreateAsync(NewRequest());

        // Assert
        Assert.InRange(created.CreatedAt, før, DateTime.Now);
    }

    [Fact]
    public async Task ShouldTrimUserNameAndEmail_WhenInputHasMellemrum()
    {
        // Arrange
        CreateUserDto request = NewRequest($"  {ValidUserName}  ", ValidPassword, $"  {ValidEmail}  ");

        // Act
        await service.CreateAsync(request);

        // Assert
        User added = users.Added.Single();
        Assert.Equal((ValidUserName, ValidEmail), (added.UserName, added.Email));
    }

    [Theory]
    [MemberData(nameof(BlankeFelter))]
    public async Task ShouldThrow_WhenEtFeltErBlankt(string userName, string password, string email)
    {
        // Arrange
        CreateUserDto request = NewRequest(userName, password, email);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(request));
    }

    [Theory]
    [MemberData(nameof(BlankeFelter))]
    public async Task ShouldNotStoreTheUser_WhenEtFeltErBlankt(string userName, string password, string email)
    {
        // Arrange
        CreateUserDto request = NewRequest(userName, password, email);

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(request));

        // Assert
        Assert.Empty(users.Added);
    }

    [Fact]
    public async Task ShouldThrow_WhenUserNameErOptaget()
    {
        // Arrange
        CreateUserDto request = NewRequest(userName: TakenUserName);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task ShouldThrow_WhenEmailErOptaget()
    {
        // Arrange
        CreateUserDto request = NewRequest(email: TakenEmail);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task ShouldReturnTheUser_WhenIdFindes()
    {
        // Arrange
        int id = 1;

        // Act
        UserDto found = await service.GetSingleAsync(id);

        // Assert
        Assert.Equal(TakenUserName, found.UserName);
    }

    [Fact]
    public async Task ShouldThrow_WhenIdIkkeFindes()
    {
        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetSingleAsync(42));
    }

    [Fact]
    public async Task ShouldReturnEveryUser_WhenDerIkkeErEtFilter()
    {
        // Arrange
        await service.CreateAsync(NewRequest());
        int expectedCount = 2;

        // Act
        int count = service.GetMany().Count();

        // Assert
        Assert.Equal(expectedCount, count);
    }

    [Theory]
    [InlineData("mad")]
    [InlineData("MAD")]
    [InlineData("ads")]
    public async Task ShouldFilterOnUserName_WhenDerErEtFilter(string filter)
    {
        // Arrange
        await service.CreateAsync(NewRequest());

        // Act
        List<UserDto> found = service.GetMany(filter).ToList();

        // Assert
        Assert.Equal(ValidUserName, Assert.Single(found).UserName);
    }

    [Fact]
    public async Task ShouldKeepCreatedAt_WhenUserOpdateres()
    {
        // Arrange
        DateTime expected = new(2026, 1, 1);
        UpdateUserDto request = new() { UserName = "nytnavn", Password = "nytpass", Email = "nyt@x.dk" };

        // Act
        UserDto updated = await service.UpdateAsync(1, request);

        // Assert
        Assert.Equal(expected, updated.CreatedAt);
    }

    [Fact]
    public async Task ShouldAcceptEgetUserName_WhenUserOpdateres()
    {
        // Arrange
        UpdateUserDto request = new() { UserName = TakenUserName, Password = "nytpass", Email = TakenEmail };

        // Act
        UserDto updated = await service.UpdateAsync(1, request);

        // Assert
        Assert.Equal(TakenUserName, updated.UserName);
    }

    [Fact]
    public async Task ShouldThrow_WhenUserNameTilhørerEnAndenBruger()
    {
        // Arrange
        await service.CreateAsync(NewRequest());
        UpdateUserDto request = new() { UserName = TakenUserName, Password = ValidPassword, Email = ValidEmail };

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(2, request));
    }
}
