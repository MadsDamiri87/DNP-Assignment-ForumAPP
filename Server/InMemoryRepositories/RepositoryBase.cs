using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public abstract class RepositoryBase<T> : IRepository<T> where T : IEntity
{
    private readonly List<T> entities = new();
    
    public Task<T> AddAsync(T entity)
    {
        entity.Id = entities.Any() ? entities.Max(e => e.Id) + 1 : 1;
        
        entities.Add(entity);
        
        return Task.FromResult(entity);
    }
    
    public Task UpdateAsync(T entity)
    {
        T? existingEntity = 
            entities.SingleOrDefault(e => e.Id == entity.Id);
        if (existingEntity is null)
        {
            throw new InvalidOperationException(
                $"The {typeof(T).Name} with id: {entity.Id} was not found.");
            
        }
        entities.Remove(existingEntity);
        entities.Add(entity);
        
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(int id)
    {
        T? entityToRemove =
            entities.SingleOrDefault(e => e.Id == id);

        if (entityToRemove is null)
        {
            throw new InvalidOperationException(
                $"The {typeof(T).Name} with id: {id} was not found.");
        }
        
        entities.Remove(entityToRemove);
        return Task.CompletedTask;
    }
    
    public Task<T> GetSingleAsync(int id)
    {
        T? existingEntity = 
            entities.SingleOrDefault(e => e.Id == id);

        if (existingEntity is null)
        {
            throw new InvalidOperationException(
                $"The {typeof(T).Name} with id: {id} was not found.");            
        }
        
        return Task.FromResult(existingEntity);
    }
    
    public IQueryable<T> GetManyAsync()
    {
        return entities.AsQueryable();
    }
}