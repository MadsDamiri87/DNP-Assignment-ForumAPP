using System.Data.Common;
using ApiContracts;
using EfcRepositories.Repositories;
using Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ServiceContracts;
using Services;

namespace Tests.IntegrationTests;

// Bruger en isoleret SQLite-database og de rigtige repositories uden at berøre app.db.
public class EfcServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly AsyncQueryGuard guard = new();
    private TestContext context = null!;
    private UserService users = null!;
    private PostService posts = null!;
    private CommentService comments = null!;
    private SubForumService forums = null!;
    private AuthService auth = null!;
    private static readonly Caller Owner = new(1, true);

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        context = new TestContext(connection, guard);
        await context.Database.EnsureCreatedAsync();
        var userRepo = new EfcUserRepository(context);
        var postRepo = new EfcPostRepository(context);
        var commentRepo = new EfcCommentRepository(context);
        var forumRepo = new EfcSubForumRepository(context);
        users = new UserService(userRepo);
        posts = new PostService(postRepo, userRepo, forumRepo, commentRepo);
        comments = new CommentService(commentRepo, postRepo, userRepo);
        forums = new SubForumService(forumRepo, userRepo, postRepo);
        auth = new AuthService(userRepo);

        await userRepo.AddAsync(new User { UserName = "mads", Password = " secret ", Email = "mads@example.test", Role = "Admin" });
        await forumRepo.AddAsync(new SubForum { Name = "Blazor", Description = "Original", CreatorUserId = 1 });
        await postRepo.AddAsync(new Post { Title = "First", Body = "Body", UserId = 1, SubForumId = 1, CreatedAt = new DateTime(2026, 1, 1) });
        await postRepo.AddAsync(new Post { Title = "Second", Body = "Body", UserId = 1, CreatedAt = new DateTime(2026, 1, 1) });
        await commentRepo.AddAsync(new Comment { Body = "Comment", PostId = 1, UserId = 1 });
        context.ChangeTracker.Clear();
        guard.Enabled = true;
    }

    [Theory]
    [InlineData("user")]
    [InlineData("post")]
    [InlineData("comment")]
    [InlineData("forum")]
    public async Task ShouldUpdateTrackedEntity_WhenOwnerEdits(string entity)
    {
        switch (entity)
        {
            case "user":
                UserDto user = await users.UpdateAsync(1, new UpdateUserDto { UserName = "changed", Password = " new secret ", Email = "changed@example.test" }, Owner);
                Assert.Equal("Admin", user.Role);
                break;
            case "post":
                PostDto post = await posts.UpdateAsync(1, new UpdatePostDto { Title = "changed", Body = "changed", SubForumId = 1 }, Owner);
                Assert.Equal(1, post.CommentCount);
                break;
            case "comment":
                CommentDto comment = await comments.UpdateAsync(1, new UpdateCommentDto { Body = "changed" }, Owner);
                Assert.Equal(1, comment.PostId);
                break;
            case "forum":
                SubForumDto forum = await forums.UpdateAsync(1, new UpdateSubForumDto { Name = " changed ", Description = " changed " }, Owner);
                Assert.Equal("changed", forum.Name);
                break;
        }

        context.ChangeTracker.Clear();
        if (entity == "user") Assert.Equal(" new secret ", (await context.Users.SingleAsync()).Password);
        if (entity == "post") Assert.Equal("changed", (await context.Posts.SingleAsync(p => p.Id == 1)).Title);
        if (entity == "comment") Assert.Equal("changed", (await context.Comments.SingleAsync()).Body);
        if (entity == "forum") Assert.Equal("changed", (await context.SubForums.SingleAsync()).Description);
    }

    [Fact]
    public async Task ShouldFilterAndMapLists_WhenUsingSqlite()
    {
        Assert.Single(await users.GetManyAsync("MAD"));
        PostDto post = Assert.Single(await posts.GetManyAsync(titleContains: "FIRST", userId: 1, userNameContains: "MAD", subForumId: 1));
        Assert.Equal(1, post.CommentCount);
        Assert.Equal("mads", post.AuthorUserName);
        Assert.Single(await comments.GetManyAsync(postId: 1, userId: 1, userNameContains: "MAD"));
        Assert.Single(await forums.GetManyAsync("BLA", 1));
        Assert.Empty(await posts.GetManyAsync(titleContains: "missing"));
    }

    [Fact]
    public async Task ShouldReadAuthorAndCountsAsync_WhenFetchingSingleEntities()
    {
        Assert.Equal(1, (await posts.GetSingleAsync(1)).CommentCount);
        Assert.Equal("mads", (await comments.GetSingleAsync(1)).AuthorUserName);
        Assert.Equal("mads", (await forums.GetSingleAsync(1)).CreatorUserName);
    }

    [Fact]
    public async Task ShouldValidateNamesAndPasswordsAsync_WhenCreatingOrLoggingIn()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => users.CreateAsync(new CreateUserDto { UserName = "MADS", Password = "secret", Email = "new@example.test" }));
        await Assert.ThrowsAsync<ArgumentException>(() => users.CreateAsync(new CreateUserDto { UserName = "other", Password = "secret", Email = "MADS@EXAMPLE.TEST" }));
        await Assert.ThrowsAsync<ArgumentException>(() => forums.CreateAsync(new CreateSubForumDto { Name = "BLAZOR", Description = "Duplicate" }, 1));
        Assert.NotNull(await auth.ValidateUserAsync(new AuthDto { UserName = "MADS", Password = " secret " }));
        Assert.Null(await auth.ValidateUserAsync(new AuthDto { UserName = "mads", Password = "wrong" }));
        UserDto created = await users.CreateAsync(new CreateUserDto { UserName = "other", Password = "secret", Email = "other@example.test" });
        Assert.True(created.Id > 1);
    }

    [Fact]
    public async Task ShouldKeepPosts_WhenSubForumIsDeleted()
    {
        await forums.DeleteAsync(1, Owner);
        Assert.Empty(await context.SubForums.ToListAsync());
        Assert.Equal(2, await context.Posts.CountAsync());
        Assert.Null((await context.Posts.SingleAsync(p => p.Id == 1)).SubForumId);
    }

    [Fact]
    public async Task ShouldRemoveComments_WhenPostIsDeleted()
    {
        await posts.DeleteAsync(1, Owner);
        Assert.Equal(1, await context.Posts.CountAsync());
        Assert.Empty(await context.Comments.ToListAsync());
    }

    [Theory]
    [InlineData(2, false, true)]
    [InlineData(3, true, true)]
    [InlineData(1, false, true)]
    [InlineData(3, false, false)]
    public async Task ShouldCheckDeletePermission_WhenThePostBelongsToASubForum(int userId, bool isAdmin, bool allowed)
    {
        await context.Users.AddAsync(new User { UserName = "other", Password = "secret", Email = "other@example.test" });
        await context.SaveChangesAsync();
        Post post = await context.Posts.SingleAsync(candidate => candidate.Id == 1);
        post.UserId = 2;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        if (allowed)
        {
            await posts.DeleteAsync(1, new Caller(userId, isAdmin));

            Assert.False(await context.Posts.AnyAsync(candidate => candidate.Id == 1));
            Assert.Empty(await context.Comments.ToListAsync());
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => posts.DeleteAsync(1, new Caller(userId, isAdmin)));

            Assert.True(await context.Posts.AnyAsync(candidate => candidate.Id == 1));
            Assert.Single(await context.Comments.ToListAsync());
        }
    }

    [Fact]
    public async Task ShouldKeepAllContent_WhenTheSubForumCreatorDeletesTheirAccount()
    {
        User other = new() { UserName = "other", Password = "secret", Email = "other@example.test" };
        await context.Users.AddAsync(other);
        await context.SaveChangesAsync();
        await context.Posts.AddAsync(new Post
        {
            Title = "Other users post", Body = "Body", UserId = other.Id, SubForumId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await context.Comments.AddAsync(new Comment { Body = "Other users comment", UserId = other.Id, PostId = 1 });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await users.DeleteAsync(1, new Caller(1, false));
        context.ChangeTracker.Clear();

        Assert.True((await context.Users.SingleAsync(user => user.Id == 1)).IsDeleted);
        Assert.Equal(2, await context.Users.CountAsync());
        Assert.Equal(3, await context.Posts.CountAsync());
        Assert.Equal(2, await context.Comments.CountAsync());
        Assert.Single(await context.SubForums.ToListAsync());
        Assert.Equal(1, (await context.Posts.SingleAsync(post => post.UserId == other.Id)).SubForumId);

        Assert.Single(await users.GetManyAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => users.GetSingleAsync(1));
        Assert.Null(await auth.ValidateUserAsync(new AuthDto { UserName = "mads", Password = " secret " }));
        Assert.NotNull(await auth.ValidateUserAsync(new AuthDto { UserName = "other", Password = "secret" }));

        Assert.Equal(DeletedUser.UserName, (await posts.GetSingleAsync(1)).AuthorUserName);
        Assert.Equal(DeletedUser.UserName, (await comments.GetSingleAsync(1)).AuthorUserName);
        Assert.Equal(DeletedUser.UserName, (await forums.GetSingleAsync(1)).CreatorUserName);
        Assert.All((await posts.GetManyAsync(userId: 1)), post => Assert.Equal(DeletedUser.UserName, post.AuthorUserName));
        Assert.Equal(DeletedUser.UserName, (await comments.GetManyAsync(userId: 1)).Single().AuthorUserName);
        Assert.Equal(DeletedUser.UserName, (await forums.GetManyAsync()).Single().CreatorUserName);
        Assert.Empty(await posts.GetManyAsync(userNameContains: "mads"));
        Assert.Empty(await comments.GetManyAsync(userNameContains: "mads"));
        Assert.Equal("other", (await posts.GetManyAsync(userId: other.Id)).Single().AuthorUserName);
    }

    [Theory]
    [InlineData("post")]
    [InlineData("comment")]
    [InlineData("forum")]
    public async Task ShouldRejectNewContent_WhenTheAuthorIsDeleted(string entity)
    {
        await users.DeleteAsync(1, new Caller(1, false));

        Task create = entity switch
        {
            "post" => posts.CreateAsync(new CreatePostDto { Title = "Title", Body = "Body" }, 1),
            "comment" => comments.CreateAsync(new CreateCommentDto { Body = "Body", PostId = 1 }, 1),
            _ => forums.CreateAsync(new CreateSubForumDto { Name = "Other", Description = "Description" }, 1)
        };

        await Assert.ThrowsAsync<ArgumentException>(() => create);
        Assert.Equal(2, await context.Posts.CountAsync());
        Assert.Single(await context.Comments.ToListAsync());
        Assert.Single(await context.SubForums.ToListAsync());
    }

    public async Task DisposeAsync()
    {
        await context.DisposeAsync();
        await connection.DisposeAsync();
    }

    private sealed class TestContext(SqliteConnection connection, AsyncQueryGuard guard) : EfcRepositories.AppContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseSqlite(connection).AddInterceptors(guard);
    }

    // Fejler hvis et af serviceforløbene går tilbage til et synkront databaseopslag.
    private sealed class AsyncQueryGuard : DbCommandInterceptor
    {
        public bool Enabled { get; set; }
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            if (Enabled) throw new InvalidOperationException("Et databaseopslag blev udført synkront.");
            return result;
        }
    }
}
