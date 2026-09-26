using Entities;
using Services;
using Tests.UnitTests.Fakes;
using Xunit;

namespace Tests.UnitTests.Services;

// Unit tests af PostService med fake repositories. Fokus: hvad der sker med kommentarerne, når en post slettes.
// Testnavne: Should<Resultat>_When<Betingelse>.
public class PostServiceTests
{
    private readonly FakePostRepository posts = new();
    private readonly FakeCommentRepository comments = new();
    private readonly PostService service;

    // Kører før hver test (xUnits svar på JUnits @BeforeEach).
    public PostServiceTests()
    {
        posts.Seed(NewPost(1), NewPost(2));
        comments.Seed(NewComment(1, postId: 1), NewComment(2, postId: 1), NewComment(3, postId: 2));
        service = new PostService(posts, new FakeUserRepository(), new FakeSubForumRepository(), comments);
    }

    private static Post NewPost(int id) =>
        new() { Id = id, Title = "Title", Body = "Body", UserId = 1, CreatedAt = new DateTime(2026, 1, 1) };

    private static Comment NewComment(int id, int postId) =>
        new() { Id = id, Body = "Body", PostId = postId, UserId = 1, CreatedAt = new DateTime(2026, 1, 1) };

    [Fact]
    public async Task ShouldDeleteThePost_WhenPostIsDeleted()
    {
        // Act
        await service.DeleteAsync(1);

        // Assert
        Assert.DoesNotContain(posts.Items, post => post.Id == 1);
    }

    // Regressionstest: kommentarer blev tidligere hængende uden en post.
    [Fact]
    public async Task ShouldDeleteItsComments_WhenPostIsDeleted()
    {
        // Act
        await service.DeleteAsync(1);

        // Assert
        Assert.DoesNotContain(comments.Items, comment => comment.PostId == 1);
    }

    [Fact]
    public async Task ShouldKeepOtherPostsComments_WhenPostIsDeleted()
    {
        // Act
        await service.DeleteAsync(1);

        // Assert
        Comment remaining = Assert.Single(comments.Items);
        Assert.Equal(2, remaining.PostId);
    }

    [Fact]
    public async Task ShouldNotTouchTheComments_WhenPostDoesNotExist()
    {
        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(99));

        // Assert
        Assert.Equal(3, comments.Items.Count);
    }
}
