using ApiContracts;
using Entities;
using ServiceContracts;
using Services;
using Tests.UnitTests.Fakes;
using Xunit;

namespace Tests.UnitTests.Services;

// Unit tests af PostService med fake repositories. Fokus: kommentarer ved sletning af en post, og visning af en slettet forfatter.
// Testnavne: Should<Resultat>_When<Betingelse>.
public class PostServiceTests
{
    private readonly FakePostRepository posts = new();
    private readonly FakeCommentRepository comments = new();
    private readonly FakeUserRepository users = new();
    private readonly PostService service;

    // De seedede posts har UserId = 1, så denne kalder er ejeren og ikke admin.
    private static readonly Caller Owner = new(1, false);

    // Kører før hver test (xUnits svar på JUnits @BeforeEach).
    public PostServiceTests()
    {
        posts.Seed(NewPost(1), NewPost(2));
        comments.Seed(NewComment(1, postId: 1), NewComment(2, postId: 1), NewComment(3, postId: 2));
        service = new PostService(posts, users, new FakeSubForumRepository(), comments);
    }

    private static Post NewPost(int id, int userId = 1) =>
        new() { Id = id, Title = "Title", Body = "Body", UserId = userId, CreatedAt = new DateTime(2026, 1, 1) };

    private static Comment NewComment(int id, int postId) =>
        new() { Id = id, Body = "Body", PostId = postId, UserId = 1, CreatedAt = new DateTime(2026, 1, 1) };

    [Fact]
    public async Task ShouldDeleteThePost_WhenPostIsDeleted()
    {
        // Act
        await service.DeleteAsync(1, Owner);

        // Assert
        Assert.DoesNotContain(posts.Items, post => post.Id == 1);
    }

    // Regressionstest: kommentarer blev tidligere hængende uden en post.
    [Fact]
    public async Task ShouldDeleteItsComments_WhenPostIsDeleted()
    {
        // Act
        await service.DeleteAsync(1, Owner);

        // Assert
        Assert.DoesNotContain(comments.Items, comment => comment.PostId == 1);
    }

    [Fact]
    public async Task ShouldKeepOtherPostsComments_WhenPostIsDeleted()
    {
        // Act
        await service.DeleteAsync(1, Owner);

        // Assert
        Comment remaining = Assert.Single(comments.Items);
        Assert.Equal(2, remaining.PostId);
    }

    [Fact]
    public async Task ShouldNotTouchTheComments_WhenPostDoesNotExist()
    {
        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(99, Owner));

        // Assert
        Assert.Equal(3, comments.Items.Count);
    }

    // En slettet bruger må ikke tage sine posts med sig, men vises med et neutralt navn.
    [Fact]
    public async Task ShouldShowTheDeletedUserName_WhenTheAuthorDoesNotExistAnymore()
    {
        // Act
        PostDto post = await service.GetSingleAsync(1);

        // Assert
        Assert.Equal(DeletedUser.UserName, post.AuthorUserName);
    }

    [Fact]
    public void ShouldShowTheDeletedUserName_WhenManyPostsAreFetchedAndTheAuthorIsGone()
    {
        // Act
        List<PostDto> found = service.GetMany().ToList();

        // Assert
        Assert.All(found, post => Assert.Equal(DeletedUser.UserName, post.AuthorUserName));
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
        PostDto post = await service.GetSingleAsync(1);

        // Assert
        Assert.Equal("mads", post.AuthorUserName);
    }

    // Ejerskab: kun forfatteren eller en admin må ændre en post.
    // Post 5 er skrevet af bruger 7, så postens id og forfatterens id er forskellige.
    // Det afslører et tjek, der ved en fejl sammenligner postens eget id med brugerens.
    [Fact]
    public async Task ShouldDeleteThePost_WhenTheAuthorDeletesIt()
    {
        // Arrange
        posts.Seed(NewPost(5, userId: 7));

        // Act
        await service.DeleteAsync(5, new Caller(7, false));

        // Assert
        Assert.DoesNotContain(posts.Items, post => post.Id == 5);
    }

    [Fact]
    public async Task ShouldThrowUnauthorized_WhenAnotherUserDeletesThePost()
    {
        // Arrange
        posts.Seed(NewPost(5, userId: 7));

        // Act
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeleteAsync(5, new Caller(3, false)));

        // Assert
        Assert.Contains(posts.Items, post => post.Id == 5);
    }

    [Fact]
    public async Task ShouldDeleteThePost_WhenAnAdminDeletesAnotherUsersPost()
    {
        // Arrange
        posts.Seed(NewPost(5, userId: 7));

        // Act
        await service.DeleteAsync(5, new Caller(3, true));

        // Assert
        Assert.DoesNotContain(posts.Items, post => post.Id == 5);
    }

    [Fact]
    public async Task ShouldThrowUnauthorized_WhenAnotherUserUpdatesThePost()
    {
        // Arrange
        posts.Seed(NewPost(5, userId: 7));
        UpdatePostDto request = new() { Title = "Kapret", Body = "Body", SubForumId = null };

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.UpdateAsync(5, request, new Caller(3, false)));
    }
}
