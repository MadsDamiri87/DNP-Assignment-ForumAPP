using Xunit;

namespace Tests.IntegrationTests;

// Integration tests: DataSeeder together with the real in-memory repositories.
// The data is seeded once for the whole class by SeededRepositoriesFixture (xUnit's equivalent of
// JUnit's @BeforeAll), because every test only reads it. Test names: Should<Result>_When<Condition>.
//
// Assignment 2: "Each of your repositories must create some initial dummy data, say 3-5 entities".
// The valid range for the count is [3, 5] - InRange checks both boundaries.
public class DataSeederTests : IClassFixture<SeededRepositoriesFixture>
{
    private const int MinimumEntities = 3;
    private const int MaximumEntities = 5;

    private readonly SeededRepositoriesFixture seeded;

    public DataSeederTests(SeededRepositoriesFixture seeded)
    {
        this.seeded = seeded;
    }

    [Fact]
    public void ShouldCreateThreeToFiveUsers_WhenSeeded()
    {
        // Arrange + Act: done once by SeededRepositoriesFixture
        int count = seeded.Users.GetManyAsync().Count();

        // Assert
        Assert.InRange(count, MinimumEntities, MaximumEntities);
    }

    [Fact]
    public void ShouldCreateThreeToFiveSubForums_WhenSeeded()
    {
        // Arrange + Act: done once by SeededRepositoriesFixture
        int count = seeded.SubForums.GetManyAsync().Count();

        // Assert
        Assert.InRange(count, MinimumEntities, MaximumEntities);
    }

    [Fact]
    public void ShouldCreateThreeToFivePosts_WhenSeeded()
    {
        // Arrange + Act: done once by SeededRepositoriesFixture
        int count = seeded.Posts.GetManyAsync().Count();

        // Assert
        Assert.InRange(count, MinimumEntities, MaximumEntities);
    }

    [Fact]
    public void ShouldCreateThreeToFiveComments_WhenSeeded()
    {
        // Arrange + Act: done once by SeededRepositoriesFixture
        int count = seeded.Comments.GetManyAsync().Count();

        // Assert
        Assert.InRange(count, MinimumEntities, MaximumEntities);
    }

    [Fact]
    public void ShouldOnlyReferenceExistingUsers_WhenSubForumsAreSeeded()
    {
        // Arrange
        List<int> userIds = seeded.Users.GetManyAsync().Select(u => u.Id).ToList();

        // Act
        List<int> referencedUserIds = seeded.SubForums.GetManyAsync().Select(s => s.CreatorUserId).ToList();

        // Assert
        Assert.All(referencedUserIds, id => Assert.Contains(id, userIds));
    }

    [Fact]
    public void ShouldOnlyReferenceExistingUsers_WhenPostsAreSeeded()
    {
        // Arrange
        List<int> userIds = seeded.Users.GetManyAsync().Select(u => u.Id).ToList();

        // Act
        List<int> referencedUserIds = seeded.Posts.GetManyAsync().Select(p => p.UserId).ToList();

        // Assert
        Assert.All(referencedUserIds, id => Assert.Contains(id, userIds));
    }

    [Fact]
    public void ShouldOnlyReferenceExistingSubForums_WhenPostsAreSeeded()
    {
        // Arrange
        List<int> subForumIds = seeded.SubForums.GetManyAsync().Select(s => s.Id).ToList();

        // Act - posts without a subforum (null) are allowed and skipped
        List<int> referencedSubForumIds = seeded.Posts.GetManyAsync().Select(p => p.SubForumId).OfType<int>().ToList();

        // Assert
        Assert.All(referencedSubForumIds, id => Assert.Contains(id, subForumIds));
    }

    [Fact]
    public void ShouldOnlyReferenceExistingPosts_WhenCommentsAreSeeded()
    {
        // Arrange
        List<int> postIds = seeded.Posts.GetManyAsync().Select(p => p.Id).ToList();

        // Act
        List<int> referencedPostIds = seeded.Comments.GetManyAsync().Select(c => c.PostId).ToList();

        // Assert
        Assert.All(referencedPostIds, id => Assert.Contains(id, postIds));
    }

    [Fact]
    public void ShouldOnlyReferenceExistingUsers_WhenCommentsAreSeeded()
    {
        // Arrange
        List<int> userIds = seeded.Users.GetManyAsync().Select(u => u.Id).ToList();

        // Act
        List<int> referencedUserIds = seeded.Comments.GetManyAsync().Select(c => c.UserId).ToList();

        // Assert
        Assert.All(referencedUserIds, id => Assert.Contains(id, userIds));
    }
}
