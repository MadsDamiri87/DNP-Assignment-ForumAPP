using Entities;
using FileRepository;
using InMemoryRepositories;
using Xunit;

namespace Tests.IntegrationTests;


public class FileRepositoryTests : IDisposable
{
    private readonly string folder;

    // Kører før hver test (JUnit: @BeforeEach).
    public FileRepositoryTests()
    {
        folder = Path.Combine(Path.GetTempPath(), "ForumAppFileRepositoryTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
    }

    // Kører efter hver test (JUnit: @AfterEach).
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

    // Det er hele pointen med opgaven: data overlever objektet, der skrev den.
    [Fact]
    public async Task ShouldStillFindTheEntity_WhenAnotherRepositoryReadsTheSameFile()
    {
        // Arrange
        PostFileRepository firstSession = NewRepository();
        Post created = await firstSession.AddAsync(NewPost("Written in the first session"));

        // Act - et nyt repository, som om programmet var genstartet
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
        int count = NewRepository().GetMany().Count();

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
        Assert.Empty(NewRepository().GetMany());
    }

    [Theory]
    [InlineData(0)]  // BVA: lige under det laveste mulige id
    [InlineData(2)]  // BVA: lige over det eneste eksisterende id
    [InlineData(42)] // EP: repræsentant for "intet sådant id"
    public async Task ShouldThrow_WhenIdDoesNotExist(int id)
    {
        // Arrange
        PostFileRepository repository = NewRepository();
        await repository.AddAsync(NewPost());

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetSingleAsync(id));
    }

    // Liskov Substitution Principle: CLI'en kender kun IPostRepository, så fil-versionen skal
    // fejle på præcis samme måde som in-memory-versionen - samme exception-type, samme besked.
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

    // Repositories gemmer deres filer i en Data-mappe, som ikke findes ved første kørsel.
    [Fact]
    public void ShouldCreateTheFolder_WhenTheDataFolderDoesNotExistYet()
    {
        // Arrange
        string dataFolder = Path.Combine(folder, "Data");
        string dataFile = Path.Combine(dataFolder, "posts.json");

        // Act
        _ = new PostFileRepository(dataFile);

        // Assert
        Assert.True(File.Exists(dataFile));
    }

    // Regressionstest: et slettet id blev tidligere givet videre til den næste entity.
    [Fact]
    public async Task ShouldNotReuseId_WhenNewestEntityWasDeleted()
    {
        // Arrange
        PostFileRepository repository = NewRepository();
        await repository.AddAsync(NewPost("First"));
        Post newest = await repository.AddAsync(NewPost("Second"));
        await repository.DeleteAsync(newest.Id);

        // Act
        Post created = await repository.AddAsync(NewPost("Third"));

        // Assert
        Assert.NotEqual(newest.Id, created.Id);
    }

    [Fact]
    public async Task ShouldNotReuseId_WhenAnotherRepositoryReadsTheSameFile()
    {
        // Arrange
        PostFileRepository firstSession = NewRepository();
        await firstSession.AddAsync(NewPost("First"));
        Post newest = await firstSession.AddAsync(NewPost("Second"));
        await firstSession.DeleteAsync(newest.Id);

        // Act - et nyt repository, som om programmet var genstartet
        Post created = await NewRepository().AddAsync(NewPost("Third"));

        // Assert
        Assert.NotEqual(newest.Id, created.Id);
    }

    // Filer fra før tælleren fandtes, skal stadig virke.
    [Fact]
    public async Task ShouldContinueAfterTheHighestId_WhenTheFileExistsWithoutAnIdCounter()
    {
        // Arrange
        await File.WriteAllTextAsync(PostsFile,
            "[{\"Id\":7,\"Title\":\"Old\",\"Body\":\"B\",\"UserId\":1,\"SubForumId\":null,\"CreatedAt\":\"2026-01-01T00:00:00\"}]");
        int expectedId = 8;

        // Act
        Post created = await NewRepository().AddAsync(NewPost());

        // Assert
        Assert.Equal(expectedId, created.Id);
    }
}
