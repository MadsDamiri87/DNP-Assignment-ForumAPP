using InMemoryRepositories;
using Xunit;

namespace Tests.IntegrationTests;

// Shared, read-only test data for DataSeederTests. xUnit creates this once for the whole test class
// and calls InitializeAsync before the first test (JUnit: @BeforeAll) and DisposeAsync after the last
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
