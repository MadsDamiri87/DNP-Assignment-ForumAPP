using Entities;
using FileRepository;
using InMemoryRepositories;
using Xunit;

namespace Tests.IntegrationTests;


public class FileRepositoryTests : IDisposable
{
    private readonly string folder;

    // Runs before every test (JUnit: @BeforeEach).
    public FileRepositoryTests()
    {
        folder = Path.Combine(Path.GetTempPath(), "ForumAppFileRepositoryTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
    }

    // Runs after every test (JUnit: @AfterEach).
    public void Dispose() => Directory.Delete(folder, recursive: true);

    private string PostsFile => Path.Combine(folder, "posts.json");

    private PostFileRepository NewRepository() => new(PostsFile);

    private static Post NewPost(string title = "Title", int userId = 1) => new()
    {
        Title = title,
        Body = "Body",
        UserId = userId,
        CreatedAt = DateTime.Now
    };

    [Fact]
    public void ShouldCreateTheFileWithAnEmptyList_WhenRepositoryIsCreatedAndNoFileExists()
    {
        // Arrange
        string expectedContent = "[]";

        // Act
        NewRepository();

        // Assert
        Assert.Equal(expectedContent, File.ReadAllText(PostsFile));
    }

    [Fact]
    public async Task ShouldAssignIdOne_WhenFileIsEmpty()
    {
        // Arrange
        PostFileRepository repository = NewRepository();
        int expectedId = 1;

        // Act
        Post created = await repository.AddAsync(NewPost());

        // Assert
        Assert.Equal(expectedId, created.Id);
    }

    [Fact]
    public async Task ShouldAssignNextId_WhenFileAlreadyHasEntities()
    {
        // Arrange
        PostFileRepository repository = NewRepository();
        await repository.AddAsync(NewPost("First"));
        await repository.AddAsync(NewPost("Second"));
        int expectedId = 3;

        // Act
        Post created = await repository.AddAsync(NewPost("Third"));

        // Assert
        Assert.Equal(expectedId, created.Id);
    }

    [Fact]
    public async Task ShouldWriteTheEntityToTheFile_WhenEntityIsAdded()
    {
        // Arrange
        PostFileRepository repository = NewRepository();
        string expectedTitle = "Stored on disk";

        // Act
        await repository.AddAsync(NewPost(expectedTitle));

        // Assert
        Assert.Contains(expectedTitle, await File.ReadAllTextAsync(PostsFile));
    }

    // This is the point of the assignment: the data outlives the object that wrote it.
    [Fact]
    public async Task ShouldStillFindTheEntity_WhenAnotherRepositoryReadsTheSameFile()
    {
        // Arrange
        PostFileRepository firstSession = NewRepository();
        Post created = await firstSession.AddAsync(NewPost("Written in the first session"));

        // Act - a new repository, as if the application had been restarted
        PostFileRepository secondSession = NewRepository();
        Post found = await secondSession.GetSingleAsync(created.Id);

        // Assert
        Assert.Equal("Written in the first session", found.Title);
    }

    [Fact]
    public async Task ShouldKeepTheNextIdCorrect_WhenAnotherRepositoryReadsTheSameFile()
    {
        // Arrange
        PostFileRepository firstSession = NewRepository();
        await firstSession.AddAsync(NewPost("First"));
        int expectedId = 2;

        // Act
        PostFileRepository secondSession = NewRepository();
        Post created = await secondSession.AddAsync(NewPost("Second"));

        // Assert
        Assert.Equal(expectedId, created.Id);
    }

    [Fact]
    public async Task ShouldReturnEveryEntityFromTheFile_WhenGetManyAsyncIsCalled()
    {
        // Arrange
        PostFileRepository repository = NewRepository();
        await repository.AddAsync(NewPost("First"));
        await repository.AddAsync(NewPost("Second"));
        int expectedCount = 2;

        // Act
        int count = NewRepository().GetManyAsync().Count();

        // Assert
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public async Task ShouldReplaceTheStoredEntity_WhenUpdatingAnExistingId()
    {
        // Arrange
        PostFileRepository repository = NewRepository();
        Post created = await repository.AddAsync(NewPost("Original"));
        string expectedTitle = "Updated";

        // Act
        await repository.UpdateAsync(new Post
        {
            Id = created.Id, Title = expectedTitle, Body = "Body", UserId = 1, CreatedAt = DateTime.Now
        });

        // Assert
        Post stored = await NewRepository().GetSingleAsync(created.Id);
        Assert.Equal(expectedTitle, stored.Title);
    }

    [Fact]
    public async Task ShouldRemoveTheEntityFromTheFile_WhenDeletingAnExistingId()
    {
        // Arrange
        PostFileRepository repository = NewRepository();
        Post created = await repository.AddAsync(NewPost());

        // Act
        await repository.DeleteAsync(created.Id);

        // Assert
        Assert.Empty(NewRepository().GetManyAsync());
    }

    [Theory]
    [InlineData(0)]  // BVA: just below the lowest possible id
    [InlineData(2)]  // BVA: just above the only existing id
    [InlineData(42)] // EP: representative of "no such id"
    public async Task ShouldThrow_WhenIdDoesNotExist(int id)
    {
        // Arrange
        PostFileRepository repository = NewRepository();
        await repository.AddAsync(NewPost());

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetSingleAsync(id));
    }

    // Liskov Substitution Principle: the CLI only knows IPostRepository, so the file version must
    // fail in exactly the same way as the in-memory version - same exception type, same message.
    [Fact]
    public async Task ShouldFailLikeTheInMemoryRepository_WhenIdDoesNotExist()
    {
        // Arrange
        PostFileRepository fileRepository = NewRepository();
        var inMemoryRepository = new PostInMemoryRepository();

        // Act
        var fromFile = await Assert.ThrowsAsync<InvalidOperationException>(
            () => fileRepository.GetSingleAsync(42));
        var fromMemory = await Assert.ThrowsAsync<InvalidOperationException>(
            () => inMemoryRepository.GetSingleAsync(42));

        // Assert
        Assert.Equal(fromMemory.Message, fromFile.Message);
    }
}
