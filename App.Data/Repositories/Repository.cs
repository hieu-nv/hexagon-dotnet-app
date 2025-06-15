using App.Core.Entities;
using App.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Data.Repositories;

/// <summary>
/// Base implementation of the repository pattern for Entity Framework Core.
/// </summary>
/// <typeparam name="T">The entity type that inherits from Entity.</typeparam>
/// <typeparam name="K">The type of the entity's key.</typeparam>
public class Repository<T, K>(AppDbContext dbContext) : IRepository<T, K>
    where T : Entity<K>
{
    protected AppDbContext DbContext { get; private set; } = dbContext;

    public async Task<T> CreateAsync(T entity)
    {
        DbContext.Set<T>().Add(entity);
        await DbContext.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        DbContext.Entry(entity).State = EntityState.Modified;
        await DbContext.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(K id)
    {
        var entity = await FindByIdAsync(id).ConfigureAwait(false);
        if (entity == null)
            return false;

        DbContext.Set<T>().Remove(entity);
        await DbContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<IEnumerable<T>> FindAllAsync()
    {
        return await DbContext.Set<T>().ToListAsync().ConfigureAwait(false);
    }

    public async Task<T?> FindByIdAsync(K id)
    {
        return await DbContext.Set<T>().FindAsync(id).ConfigureAwait(false);
    }
}
