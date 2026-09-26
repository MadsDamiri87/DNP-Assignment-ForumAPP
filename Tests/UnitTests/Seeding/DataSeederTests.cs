using Entities;
using InMemoryRepositories;
using Moq;
using RepositoryContracts;
using Xunit;

namespace Tests.UnitTests.Seeding;

// Unit tests af DataSeeder med mockede repositories (Moq).
//
// Hvorfor mocks her: DataSeeders opgave er at kalde AddAsync og koble entities sammen via de id'er, som
// AddAsync returnerer. De rigtige repositories giver 1, 2, 3 ..., så en seeder, der hardcodede
// "UserId = 1", stadig ville bestå integrationstestene - ved et tilfælde. Disse mocks giver id'er
// fra 100, 200, 300 og 400 i stedet, så kun en seeder, der rigtigt bruger de returnerede entities, består.
//
// Setup(...) får mocken til at fungere som en stub (den returnerer noget); Verify(...) gør den til en mock
// (den tjekker, hvilke kald der blev lavet). Testnavne: Should<Resultat>_When<Betingelse>.
public class DataSeederTests
{
    private const int MinimumEntities = 3;
    private const int MaximumEntities = 5;

    private readonly Mock<IUserRepository> users = new();
    private readonly Mock<ISubForumRepository> subForums = new();
    private readonly Mock<IPostRepository> posts = new();
    private readonly Mock<ICommentRepository> comments = new();

    private readonly List<int> returnedUserIds = new();
    private readonly List<int> returnedSubForumIds = new();
    private readonly List<int> returnedPostIds = new();

    private readonly DataSeeder seeder;

    // Kører før hver test (JUnit: @BeforeEach). Hvert AddAsync er stubbet til at give entity'en det
    // næste id fra sit eget interval og returnere den - som et rigtigt repository, bare med andre id'er.
    public DataSeederTests()
    {
        users.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => AssignId(user, 100, returnedUserIds));
        subForums.Setup(r => r.AddAsync(It.IsAny<SubForum>()))
            .ReturnsAsync((SubForum subForum) => AssignId(subForum, 200, returnedSubForumIds));
        posts.Setup(r => r.AddAsync(It.IsAny<Post>()))
            .ReturnsAsync((Post post) => AssignId(post, 300, returnedPostIds));
        comments.Setup(r => r.AddAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment comment) => AssignId(comment, 400, new List<int>()));

        seeder = new DataSeeder(users.Object, posts.Object, comments.Object, subForums.Object);
    }

    private static T AssignId<T>(T entity, int firstId, List<int> returnedIds) where T : IEntity
    {
        entity.Id = firstId + returnedIds.Count;
        returnedIds.Add(entity.Id);
        return entity;
    }

    // BVA på antallet af kald: gyldigt interval er [3, 5], og Times.Between tjekker begge grænser.
    [Fact]
    public async Task ShouldAddThreeToFiveUsers_WhenSeeding()
    {
        // Arrange: gøres i konstruktøren

        // Act
        await seeder.SeedAsync();

        // Assert
        users.Verify(r => r.AddAsync(It.IsAny<User>()),
            Times.Between(MinimumEntities, MaximumEntities, Moq.Range.Inclusive));
    }

    [Fact]
    public async Task ShouldAddThreeToFiveSubForums_WhenSeeding()
    {
        // Act
        await seeder.SeedAsync();

        // Assert
        subForums.Verify(r => r.AddAsync(It.IsAny<SubForum>()),
            Times.Between(MinimumEntities, MaximumEntities, Moq.Range.Inclusive));
    }

    [Fact]
    public async Task ShouldAddThreeToFivePosts_WhenSeeding()
    {
        // Act
        await seeder.SeedAsync();

        // Assert
        posts.Verify(r => r.AddAsync(It.IsAny<Post>()),
            Times.Between(MinimumEntities, MaximumEntities, Moq.Range.Inclusive));
    }

    [Fact]
    public async Task ShouldAddThreeToFiveComments_WhenSeeding()
    {
        // Act
        await seeder.SeedAsync();

        // Assert
        comments.Verify(r => r.AddAsync(It.IsAny<Comment>()),
            Times.Between(MinimumEntities, MaximumEntities, Moq.Range.Inclusive));
    }

    // Koblingstestene herunder verificerer, at AddAsync ALDRIG blev kaldt med et id, som repositoryet ikke
    // har udleveret. Antalstestene ovenfor sikrer, at kaldene faktisk skete.
    [Fact]
    public async Task ShouldUseReturnedUserIds_WhenSubForumsAreAdded()
    {
        // Act
        await seeder.SeedAsync();

        // Assert
        subForums.Verify(r => r.AddAsync(It.Is<SubForum>(s => !returnedUserIds.Contains(s.CreatorUserId))),
            Times.Never);
    }

    [Fact]
    public async Task ShouldUseReturnedUserIds_WhenPostsAreAdded()
    {
        // Act
        await seeder.SeedAsync();

        // Assert
        posts.Verify(r => r.AddAsync(It.Is<Post>(p => !returnedUserIds.Contains(p.UserId))),
            Times.Never);
    }

    [Fact]
    public async Task ShouldUseReturnedSubForumIds_WhenPostsAreAdded()
    {
        // Act
        await seeder.SeedAsync();

        // Assert - en post uden subforum (null) er tilladt
        posts.Verify(r => r.AddAsync(It.Is<Post>(p =>
                p.SubForumId.HasValue && !returnedSubForumIds.Contains(p.SubForumId.Value))),
            Times.Never);
    }

    [Fact]
    public async Task ShouldUseReturnedPostIds_WhenCommentsAreAdded()
    {
        // Act
        await seeder.SeedAsync();

        // Assert
        comments.Verify(r => r.AddAsync(It.Is<Comment>(c => !returnedPostIds.Contains(c.PostId))),
            Times.Never);
    }

    [Fact]
    public async Task ShouldUseReturnedUserIds_WhenCommentsAreAdded()
    {
        // Act
        await seeder.SeedAsync();

        // Assert
        comments.Verify(r => r.AddAsync(It.Is<Comment>(c => !returnedUserIds.Contains(c.UserId))),
            Times.Never);
    }
}
