using Entities;
using RepositoryContracts;

namespace Tests.UnitTests.Fakes;

// Test doubles for the repository interfaces. The views only depend on IRepository<T>,
// so a unit test can hand them a fake instead of InMemoryRepositories. That isolates the
// view logic, and 'Added' lets a test assert that AddAsync was - or was not - called.
public class FakeRepository<T> : IRepository<T> where T : IEntity
{
    private int nextId = 1;

    public List<T> Items { get; } = new();
    public List<T> Added { get; } = new();

    public void Seed(params T[] entities)
    {
        Items.AddRange(entities);
        nextId = Items.Count == 0 ? 1 : Items.Max(e => e.Id) + 1;
    }

    public Task<T> AddAsync(T entity)
    {
        entity.Id = nextId++;
        Items.Add(entity);
        Added.Add(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity)
    {
        int index = Items.FindIndex(e => e.Id == entity.Id);
        if (index == -1)
        {
            throw new InvalidOperationException($"{typeof(T).Name} {entity.Id} not found");
        }

        Items[index] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        if (Items.RemoveAll(e => e.Id == id) == 0)
        {
            throw new InvalidOperationException($"{typeof(T).Name} {id} not found");
        }

        return Task.CompletedTask;
    }

    public Task<T> GetSingleAsync(int id)
    {
        T entity = Items.FirstOrDefault(e => e.Id == id)
                   ?? throw new InvalidOperationException($"{typeof(T).Name} {id} not found");
        return Task.FromResult(entity);
    }

    public IQueryable<T> GetManyAsync() => Items.AsQueryable();
}

public class FakeUserRepository : FakeRepository<User>, IUserRepository;

public class FakePostRepository : FakeRepository<Post>, IPostRepository;

public class FakeCommentRepository : FakeRepository<Comment>, ICommentRepository;
