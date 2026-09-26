using InMemoryRepositories;
using Xunit;

namespace Tests.IntegrationTests;

// Fælles, skrivebeskyttet testdata til DataSeederTests. xUnit opretter den én gang for hele testklassen
// og kalder InitializeAsync før den første test (JUnit: @BeforeAll) og DisposeAsync efter den sidste
// (JUnit: @AfterAll).
public class SeededRepositoriesFixture : IAsyncLifetime
{
    public UserInMemoryRepository Users { get; } = new();
    public PostInMemoryRepository Posts { get; } = new();
    public CommentInMemoryRepository Comments { get; } = new();
    public SubForumInMemoryRepository SubForums { get; } = new();

    public Task InitializeAsync() => new DataSeeder(Users, Posts, Comments, SubForums).SeedAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}
