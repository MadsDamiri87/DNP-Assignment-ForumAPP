using ApiContracts;
using Entities;
using Services;
using Tests.UnitTests.Fakes;
using Xunit;

namespace Tests.UnitTests.Services;

// Unit tests af CommentService med fake repositories. Fokus: kommentarer bliver stående, når forfatteren er slettet.
// Testnavne: Should<Resultat>_When<Betingelse>.
public class CommentServiceTests
{
    private readonly FakeCommentRepository comments = new();
    private readonly FakeUserRepository users = new();
    private readonly CommentService service;

    // Kører før hver test (xUnits svar på JUnits @BeforeEach).
    public CommentServiceTests()
    {
        comments.Seed(new Comment
        {
            Id = 1, Body = "Body", PostId = 1, UserId = 1, CreatedAt = new DateTime(2026, 1, 1)
        });
        service = new CommentService(comments, new FakePostRepository(), users);
    }

    [Fact]
    public async Task ShouldShowTheDeletedUserName_WhenTheAuthorDoesNotExistAnymore()
    {
        // Act
        CommentDto comment = await service.GetSingleAsync(1);

        // Assert
        Assert.Equal(DeletedUser.UserName, comment.AuthorUserName);
    }

    [Fact]
    public void ShouldShowTheDeletedUserName_WhenManyCommentsAreFetchedAndTheAuthorIsGone()
    {
        // Act
        List<CommentDto> found = service.GetMany().ToList();

        // Assert
        Assert.All(found, comment => Assert.Equal(DeletedUser.UserName, comment.AuthorUserName));
    }

    [Fact]
    public async Task ShouldShowTheRealUserName_WhenTheAuthorStillExists()
    {
        // Arrange
        users.Seed(new User
        {
            Id = 1, UserName = "mads", PasswordHash = "hash", Email = "mads@x.dk", CreatedAt = new DateTime(2026, 1, 1)
        });

        // Act
        CommentDto comment = await service.GetSingleAsync(1);

        // Assert
        Assert.Equal("mads", comment.AuthorUserName);
    }
}
