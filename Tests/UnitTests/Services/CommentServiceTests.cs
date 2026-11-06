using ApiContracts;
using Entities;
using ServiceContracts;
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
            Id = 1, UserName = "mads", Password = "hash", Email = "mads@x.dk", CreatedAt = new DateTime(2026, 1, 1)
        });

        // Act
        CommentDto comment = await service.GetSingleAsync(1);

        // Assert
        Assert.Equal("mads", comment.AuthorUserName);
    }

    // Ejerskab. Kommentar 5 er skrevet af bruger 7, så id og forfatter er forskellige.
    // Det afslører et tjek, der ved en fejl bruger kommentarens eget id som ejer.
    private static Comment NewComment(int id, int userId) =>
        new() { Id = id, Body = "Body", PostId = 1, UserId = userId, CreatedAt = new DateTime(2026, 1, 1) };

    [Fact]
    public async Task ShouldDeleteTheComment_WhenTheAuthorDeletesIt()
    {
        // Arrange
        comments.Seed(NewComment(5, userId: 7));

        // Act
        await service.DeleteAsync(5, new Caller(7, false));

        // Assert
        Assert.DoesNotContain(comments.Items, comment => comment.Id == 5);
    }

    [Fact]
    public async Task ShouldThrowUnauthorized_WhenAnotherUserDeletesTheComment()
    {
        // Arrange
        comments.Seed(NewComment(5, userId: 7));

        // Act
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeleteAsync(5, new Caller(3, false)));

        // Assert
        Assert.Contains(comments.Items, comment => comment.Id == 5);
    }

    [Fact]
    public async Task ShouldDeleteTheComment_WhenAnAdminDeletesIt()
    {
        // Arrange
        comments.Seed(NewComment(5, userId: 7));

        // Act
        await service.DeleteAsync(5, new Caller(3, true));

        // Assert
        Assert.DoesNotContain(comments.Items, comment => comment.Id == 5);
    }
}
