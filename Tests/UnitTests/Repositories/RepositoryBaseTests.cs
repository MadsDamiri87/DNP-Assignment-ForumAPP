using Entities;
using InMemoryRepositories;
using Xunit;

namespace Tests.UnitTests.Repositories;

// Unit tests of RepositoryBase<T>. It is abstract, so it is tested through PostInMemoryRepository.
// Black-box: designed from the repository contract in Assignment 1, not from the implementation.
// One nested class per method under test. Test names: Should<Result>_When<Condition>.
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
        // A new instance is created before every test (xUnit's equivalent of JUnit's @BeforeEach),
        // so every test starts with an empty repository.
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

        // Skipped on purpose (JUnit: @Disabled). The test describes the desired behaviour; the reason
        // shows up in the test report until the defect is fixed.
        [Fact(Skip = "Known defect: AddAsync uses Max(Id) + 1, so deleting the newest entity makes its id reusable.")]
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

    // Existing ids after arranging three posts: 1..3.
    //
    //   Partition (EP)   | Representative | BVA values | Expected
    //   id < 1           | -5             | 0          | throws
    //   1 <= id <= 3     | 2              | 1, 3       | returns the entity
    //   id > 3           | 10             | 4          | throws
    public class GetSingleAsync
    {
        private const int PostCount = 3;

        [Theory]
        [InlineData(1)] // BVA: lowest existing id
        [InlineData(2)] // EP: representative of the valid partition
        [InlineData(3)] // BVA: highest existing id
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
        [InlineData(-5)] // EP: representative of id < 1
        [InlineData(0)]  // BVA: just below the lowest existing id
        [InlineData(4)]  // BVA: just above the highest existing id
        [InlineData(10)] // EP: representative of id > 3
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
        [InlineData(0)] // BVA: just below the lowest existing id
        [InlineData(4)] // BVA: just above the highest existing id
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
        [InlineData(1)] // BVA (fence-post): the first entity
        [InlineData(2)] // EP: an entity in the middle
        [InlineData(3)] // BVA (fence-post): the last entity
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
        [InlineData(0)] // BVA: just below the lowest existing id
        [InlineData(4)] // BVA: just above the highest existing id
        public async Task ShouldThrow_WhenIdDoesNotExist(int id)
        {
            // Arrange
            PostInMemoryRepository repository = await RepositoryWithPostsAsync(PostCount);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.DeleteAsync(id));
        }
    }

    // BVA on the number of stored entities: zero, one and many.
    public class GetManyAsync
    {
        [Theory]
        [InlineData(0)] // BVA: empty repository
        [InlineData(1)] // BVA: exactly one entity
        [InlineData(3)] // EP: representative of "several entities"
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

    // Why Program.cs must create each repository once and share that instance.
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
