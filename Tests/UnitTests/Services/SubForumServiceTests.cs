using ApiContracts;
using Entities;
using ServiceContracts;
using Services;
using Tests.UnitTests.Fakes;
using Xunit;

namespace Tests.UnitTests.Services;

// Unit tests af SubForumService med fake repositories. Fokus: posts ved sletning af et subforum, og visning af en slettet opretter.
// Testnavne: Should<Resultat>_When<Betingelse>.
public class SubForumServiceTests
{
    private readonly FakeSubForumRepository subForums = new();
    private readonly FakePostRepository posts = new();
    private readonly FakeUserRepository users = new();
    private readonly SubForumService service;

    // De seedede subforums har CreatorUserId = 1, så denne kalder er ejeren og ikke admin.
    private static readonly Caller Owner = new(1, false);

    // Kører før hver test (xUnits svar på JUnits @BeforeEach).
    public SubForumServiceTests()
    {
        subForums.Seed(NewSubForum(1), NewSubForum(2));
        posts.Seed(NewPost(1, subForumId: 1), NewPost(2, subForumId: 2), NewPost(3, subForumId: null));
        service = new SubForumService(subForums, users, posts);
    }

    private static SubForum NewSubForum(int id, int creatorUserId = 1) =>
        new() { Id = id, Name = $"Subforum {id}", Description = "Description", CreatorUserId = creatorUserId };

    private static Post NewPost(int id, int? subForumId) => new()
    {
        Id = id, Title = "Title", Body = "Body", UserId = 1, SubForumId = subForumId,
        CreatedAt = new DateTime(2026, 1, 1)
    };

    [Fact]
    public async Task ShouldDeleteTheSubForum_WhenSubForumIsDeleted()
    {
        // Act
        await service.DeleteAsync(1, Owner);

        // Assert
        Assert.DoesNotContain(subForums.Items, subForum => subForum.Id == 1);
    }

    // Postens relation til subforum er valgfri (0..1), så posten overlever uden subforum.
    [Fact]
    public async Task ShouldKeepThePostsButRemoveTheirSubForum_WhenSubForumIsDeleted()
    {
        // Act
        await service.DeleteAsync(1, Owner);

        // Assert
        Post post = posts.Items.Single(p => p.Id == 1);
        Assert.Null(post.SubForumId);
    }

    [Fact]
    public async Task ShouldNotTouchPostsInOtherSubForums_WhenSubForumIsDeleted()
    {
        // Act
        await service.DeleteAsync(1, Owner);

        // Assert
        Assert.Equal(2, posts.Items.Single(p => p.Id == 2).SubForumId);
    }

    [Fact]
    public async Task ShouldThrow_WhenSubForumDoesNotExist()
    {
        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(99, Owner));
    }

    [Fact]
    public async Task ShouldShowTheDeletedUserName_WhenTheCreatorDoesNotExistAnymore()
    {
        // Act
        SubForumDto subForum = await service.GetSingleAsync(1);

        // Assert
        Assert.Equal(DeletedUser.UserName, subForum.CreatorUserName);
    }

    [Fact]
    public async Task ShouldShowTheRealUserName_WhenTheCreatorStillExists()
    {
        // Arrange
        users.Seed(new User
        {
            Id = 1, UserName = "mads", Password = "hash", Email = "mads@x.dk", CreatedAt = new DateTime(2026, 1, 1)
        });

        // Act
        SubForumDto subForum = await service.GetSingleAsync(1);

        // Assert
        Assert.Equal("mads", subForum.CreatorUserName);
    }

    // Ejerskab. Subforum 5 er oprettet af bruger 7, så id og opretter er forskellige.
    // Det afslører et tjek, der ved en fejl bruger subforummets eget id som ejer.
    [Fact]
    public async Task ShouldDeleteTheSubForum_WhenTheCreatorDeletesIt()
    {
        // Arrange
        subForums.Seed(NewSubForum(5, creatorUserId: 7));

        // Act
        await service.DeleteAsync(5, new Caller(7, false));

        // Assert
        Assert.DoesNotContain(subForums.Items, subForum => subForum.Id == 5);
    }

    [Fact]
    public async Task ShouldThrowUnauthorized_WhenAnotherUserDeletesTheSubForum()
    {
        // Arrange
        subForums.Seed(NewSubForum(5, creatorUserId: 7));

        // Act
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeleteAsync(5, new Caller(3, false)));

        // Assert
        Assert.Contains(subForums.Items, subForum => subForum.Id == 5);
    }

    [Fact]
    public async Task ShouldDeleteTheSubForum_WhenAnAdminDeletesIt()
    {
        // Arrange
        subForums.Seed(NewSubForum(5, creatorUserId: 7));

        // Act
        await service.DeleteAsync(5, new Caller(3, true));

        // Assert
        Assert.DoesNotContain(subForums.Items, subForum => subForum.Id == 5);
    }
}
