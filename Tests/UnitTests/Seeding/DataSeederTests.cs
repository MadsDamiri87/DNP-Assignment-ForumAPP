using Entities;
using InMemoryRepositories;
using Moq;
using RepositoryContracts;
using Xunit;

namespace Tests.UnitTests.Seeding;

// Unit tests of DataSeeder with mocked repositories (Moq).
//
// Why mocks here: DataSeeder's job is to call AddAsync and link the entities through the ids that
// AddAsync returns. The real repositories hand out 1, 2, 3 ..., so a seeder that hard-coded
// "UserId = 1" would still pass the integration tests - by coincidence. These mocks hand out ids
// from 100, 200, 300 and 400 instead, so only a seeder that really uses the returned entities passes.
//
// Setup(...) makes the mock act as a stub (it returns something); Verify(...) makes it a mock
// (it checks which calls were made). Test names: Should<Result>_When<Condition>.
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

    // Runs before every test (JUnit: @BeforeEach). Every AddAsync is stubbed to give the entity the
    // next id from its own range and return it - like a real repository, just with other ids.
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

    // BVA on the number of calls: the valid range is [3, 5], and Times.Between checks both boundaries.
    [Fact]
    public async Task ShouldAddThreeToFiveUsers_WhenSeeding()
    {
        // Arrange: done in the constructor

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

    // The linking tests below verify that AddAsync was NEVER called with an id the repository did not
    // hand out. The count tests above make sure these calls actually happened.
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

        // Assert - a post without a subforum (null) is allowed
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
