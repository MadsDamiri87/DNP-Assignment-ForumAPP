using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepository;
public abstract class FileRepositoryBase<T> : IRepository<T> where T : IEntity
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string filePath;

    protected FileRepositoryBase(string filePath)
    {
        this.filePath = filePath;
        
        string? folder = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    public async Task<T> AddAsync(T entity)
    {
        List<T> entities = await LoadAsync();

        entity.Id = entities.Count > 0 ? entities.Max(e => e.Id) + 1 : 1;
        entities.Add(entity);

        await SaveAsync(entities);
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        List<T> entities = await LoadAsync();

        int index = entities.FindIndex(e => e.Id == entity.Id);
        if (index == -1)
        {
            throw new InvalidOperationException(NotFoundMessage(entity.Id));
        }

        entities[index] = entity;
        await SaveAsync(entities);
    }

    public async Task DeleteAsync(int id)
    {
        List<T> entities = await LoadAsync();

        int removed = entities.RemoveAll(e => e.Id == id);
        if (removed == 0)
        {
            throw new InvalidOperationException(NotFoundMessage(id));
        }

        await SaveAsync(entities);
    }

    public async Task<T> GetSingleAsync(int id)
    {
        List<T> entities = await LoadAsync();

        return entities.SingleOrDefault(e => e.Id == id)
               ?? throw new InvalidOperationException(NotFoundMessage(id));
    }
    
    public IQueryable<T> GetManyAsync()
    {
        return LoadAsync().Result.AsQueryable();
    }

    private async Task<List<T>> LoadAsync()
    {
        string entitiesAsJson = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<T>>(entitiesAsJson) ?? [];
    }

    private async Task SaveAsync(List<T> entities)
    {
        string entitiesAsJson = JsonSerializer.Serialize(entities, SerializerOptions);
        await File.WriteAllTextAsync(filePath, entitiesAsJson);
    }

    private static string NotFoundMessage(int id) => $"The {typeof(T).Name} with id: {id} was not found.";
}
