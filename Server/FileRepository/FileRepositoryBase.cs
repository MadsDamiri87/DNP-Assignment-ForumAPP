using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepository;
public abstract class FileRepositoryBase<T> : IRepository<T> where T : IEntity
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string filePath;
    private readonly string nextIdFilePath;

    protected FileRepositoryBase(string filePath)
    {
        this.filePath = filePath;
        nextIdFilePath = Path.ChangeExtension(filePath, ".nextid");

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

        entity.Id = await ReserveNextIdAsync(entities);
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
        return Deserialize(File.ReadAllText(filePath)).AsQueryable();
    }

    private async Task<List<T>> LoadAsync()
    {
        return Deserialize(await File.ReadAllTextAsync(filePath));
    }

    private static List<T> Deserialize(string entitiesAsJson)
    {
        return JsonSerializer.Deserialize<List<T>>(entitiesAsJson) ?? [];
    }

    private async Task<int> ReserveNextIdAsync(List<T> entities)
    {
        int highestExistingId = entities.Count > 0 ? entities.Max(e => e.Id) : 0;
        int nextId = highestExistingId + 1;

        if (File.Exists(nextIdFilePath)
            && int.TryParse(await File.ReadAllTextAsync(nextIdFilePath), out int storedNextId))
        {
            nextId = Math.Max(nextId, storedNextId);
        }

        await File.WriteAllTextAsync(nextIdFilePath, (nextId + 1).ToString());
        return nextId;
    }

    private async Task SaveAsync(List<T> entities)
    {
        string entitiesAsJson = JsonSerializer.Serialize(entities, SerializerOptions);
        await File.WriteAllTextAsync(filePath, entitiesAsJson);
    }

    private static string NotFoundMessage(int id) => $"The {typeof(T).Name} with id: {id} was not found.";
}
