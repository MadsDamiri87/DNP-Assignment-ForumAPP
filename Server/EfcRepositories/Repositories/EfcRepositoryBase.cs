using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace EfcRepositories.EFCRepositories;

public abstract class EfcRepositoryBase<T> : IRepository<T> where T : class, IEntity
{
    private readonly AppContext ctx;

    protected EfcRepositoryBase(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<T> AddAsync(T entity)
    {
        await ctx.Set<T>().AddAsync(entity);
        await ctx.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        if (!await ctx.Set<T>().AnyAsync(e => e.Id == entity.Id))
        {
            throw new InvalidOperationException(NotFoundMessage(entity.Id));
        }

        ctx.Set<T>().Update(entity);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        T existing = await GetSingleAsync(id);
        
        ctx.Set<T>().Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<T> GetSingleAsync(int id)
    {
        T? entity = await ctx.Set<T>().FindAsync(id);

        if (entity is null)
        {
            throw new InvalidOperationException(NotFoundMessage(id));            
        }
        return entity;
    }

    public IQueryable<T> GetMany() => ctx.Set<T>();

    protected static string NotFoundMessage(int id) => 
        $"The {typeof(T).Name} with id: {id} was not found.";
}