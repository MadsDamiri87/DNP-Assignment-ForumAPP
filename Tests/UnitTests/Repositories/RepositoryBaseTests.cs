using Entities;
using InMemoryRepositories;
using Xunit;

namespace Tests.UnitTests.Repositories;

public class RepositoryBaseTests
{
    private static Post NewPost(string title = "Title", int userId = 1) => new()
    {
        Title = title,
        Body = "Body",
        UserId = userId,
        CreatedAt = DateTime.Now
    };

    private static async Task<PostInMemoryRepository> RepositoryWithPostsAsync(int count)
    {
        var repository = new PostInMemoryRepository();
        for (int i = 1; i <= count; i++)
        {
            await repository.AddAsync(NewPost($"Post {i}"));
        }

        return repository;
    }

    public class AddAsync
    {
        // Der oprettes en ny instans før hver test (xUnits svar på JUnits @BeforeEach),
        // så hver test starter med et tomt repository.
        private readonly PostInMemoryRepository repository = new();

        [Fact]
        public async Task ShouldAssignIdOne_WhenRepositoryIsEmpty()
        {
            // Arrange
            Post post = NewPost();
            int expectedId = 1;

            // Act
            Post created = await repository.AddAsync(post);

            // Assert
            Assert.Equal(expectedId, created.Id);
        }

        [Fact]
        public async Task ShouldAssignNextId_WhenRepositoryAlreadyHasEntities()
        {
            // Arrange
            await repository.AddAsync(NewPost("First"));
            await repository.AddAsync(NewPost("Second"));
            int expectedId = 3;

            // Act
            Post created = await repository.AddAsync(NewPost("Third"));

            // Assert
            Assert.Equal(expectedId, created.Id);
        }

        [Fact]
        public async Task ShouldReturnTheAddedEntity_WhenEntityIsAdded()
        {
            // Arrange
            Post post = NewPost();

            // Act
            Post created = await repository.AddAsync(post);

            // Assert
            Assert.Same(post, created);
        }

        [Fact]
        public async Task ShouldIgnoreIdFromCaller_WhenIdIsAlreadySet()
        {
            // Arrange
            Post post = NewPost();
            post.Id = 99;
            int expectedId = 1;

            // Act
            Post created = await repository.AddAsync(post);

            // Assert
            Assert.Equal(expectedId, created.Id);
        }

        // Regressionstest: et slettet id blev tidligere givet videre til den næste entity.
        [Fact]
        public async Task ShouldNotReuseId_WhenNewestEntityWasDeleted()
        {
            // Arrange
            await repository.AddAsync(NewPost("First"));
            Post newest = await repository.AddAsync(NewPost("Second"));
            await repository.DeleteAsync(newest.Id);
            int deletedId = newest.Id;

            // Act
            Post created = await repository.AddAsync(NewPost("Third"));

            // Assert
            Assert.NotEqual(deletedId, created.Id);
        }
    }

    public class GetSingleAsync
    {
        private const int PostCount = 3;

        [Theory]
        [InlineData(1)] // BVA: laveste eksisterende id
        [InlineData(2)] // EP: repræsentant for den gyldige partition
        [InlineData(3)] // BVA: højeste eksisterende id
        public async Task ShouldReturnEntityWithThatId_WhenIdExists(int id)
        {
            // Arrange
            PostInMemoryRepository repository = await RepositoryWithPostsAsync(PostCount);

            // Act
            Post found = await repository.GetSingleAsync(id);

            // Assert
            Assert.Equal(id, found.Id);
        }

        [Theory]
        [InlineData(-5)] // EP: repræsentant for id < 1
        [InlineData(0)]  // BVA: lige under laveste eksisterende id
        [InlineData(4)]  // BVA: lige over højeste eksisterende id
        [InlineData(10)] // EP: repræsentant for id > 3
        public async Task ShouldThrow_WhenIdDoesNotExist(int id)
        {
            // Arrange
            PostInMemoryRepository repository = await RepositoryWithPostsAsync(PostCount);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetSingleAsync(id));
        }

        [Fact]
        public async Task ShouldNameTheEntityTypeInMessage_WhenIdDoesNotExist()
        {
            // Arrange
            var repository = new UserInMemoryRepository();
            string expectedTypeName = nameof(User);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetSingleAsync(1));

            // Assert
            Assert.Contains(expectedTypeName, exception.Message);
        }
    }

    public class UpdateAsync
    {
        private const int PostCount = 3;

        [Fact]
        public async Task ShouldReplaceStoredEntity_WhenIdExists()
        {
            // Arrange
            PostInMemoryRepository repository = await RepositoryWithPostsAsync(PostCount);
            string expectedTitle = "Updated title";
            Post replacement = NewPost(expectedTitle);
            replacement.Id = 2;

            // Act
            await repository.UpdateAsync(replacement);

            // Assert
            Post stored = await repository.GetSingleAsync(replacement.Id);
            Assert.Equal(expectedTitle, stored.Title);
        }

        [Fact]
        public async Task ShouldKeepTheNumberOfEntities_WhenIdExists()
        {
            // Arrange
            PostInMemoryRepository repository = await RepositoryWithPostsAsync(PostCount);
            Post replacement = NewPost("Updated title");
            replacement.Id = 2;

            // Act
            await repository.UpdateAsync(replacement);

            // Assert
            Assert.Equal(PostCount, repository.GetManyAsync().Count());
        }

        [Theory]
        [InlineData(0)] // BVA: lige under laveste eksisterende id
        [InlineData(4)] // BVA: lige over højeste eksisterende id
        public async Task ShouldThrow_WhenIdDoesNotExist(int id)
        {
            // Arrange
            PostInMemoryRepository repository = await RepositoryWithPostsAsync(PostCount);
            Post replacement = NewPost();
            replacement.Id = id;

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateAsync(replacement));
        }
    }

    public class DeleteAsync
    {
        private const int PostCount = 3;

        [Theory]
        [InlineData(1)] // BVA (fence-post): den første entity
        [InlineData(2)] // EP: en entity i midten
        [InlineData(3)] // BVA (fence-post): den sidste entity
        public async Task ShouldRemoveTheEntity_WhenIdExists(int id)
        {
            // Arrange
            PostInMemoryRepository repository = await RepositoryWithPostsAsync(PostCount);

            // Act
            await repository.DeleteAsync(id);

            // Assert
            Assert.DoesNotContain(repository.GetManyAsync(), post => post.Id == id);
        }

        [Fact]
        public async Task ShouldKeepTheOtherEntities_WhenIdExists()
        {
            // Arrange
            PostInMemoryRepository repository = await RepositoryWithPostsAsync(PostCount);
            int expectedRemaining = PostCount - 1;

            // Act
            await repository.DeleteAsync(2);

            // Assert
            Assert.Equal(expectedRemaining, repository.GetManyAsync().Count());
        }

        [Theory]
        [InlineData(0)] // BVA: lige under laveste eksisterende id
        [InlineData(4)] // BVA: lige over højeste eksisterende id
        public async Task ShouldThrow_WhenIdDoesNotExist(int id)
        {
            // Arrange
            PostInMemoryRepository repository = await RepositoryWithPostsAsync(PostCount);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.DeleteAsync(id));
        }
    }

    // BVA på antallet af gemte entities: nul, én og flere.
    public class GetManyAsync
    {
        [Theory]
        [InlineData(0)] // BVA: tomt repository
        [InlineData(1)] // BVA: præcis én entity
        [InlineData(3)] // EP: repræsentant for "flere entities"
        public async Task ShouldReturnEveryEntity_WhenRepositoryHasThatManyEntities(int count)
        {
            // Arrange
            PostInMemoryRepository repository = await RepositoryWithPostsAsync(count);

            // Act
            int returned = repository.GetManyAsync().Count();

            // Assert
            Assert.Equal(count, returned);
        }

        [Fact]
        public async Task ShouldAllowFiltering_WhenLinqWhereIsApplied()
        {
            // Arrange
            var repository = new PostInMemoryRepository();
            await repository.AddAsync(NewPost("By user 1", userId: 1));
            await repository.AddAsync(NewPost("By user 2", userId: 2));
            await repository.AddAsync(NewPost("Also by user 2", userId: 2));
            int expectedCount = 2;

            // Act
            int count = repository.GetManyAsync().Count(post => post.UserId == 2);

            // Assert
            Assert.Equal(expectedCount, count);
        }
    }

    // Hvorfor Program.cs kun må oprette hvert repository én gang og dele den instans.
    public class SeparateInstances
    {
        [Fact]
        public async Task ShouldNotSeeEntities_WhenTheyWereAddedToAnotherInstance()
        {
            // Arrange
            var first = new PostInMemoryRepository();
            var second = new PostInMemoryRepository();

            // Act
            await first.AddAsync(NewPost());

            // Assert
            Assert.Empty(second.GetManyAsync());
        }
    }
}
