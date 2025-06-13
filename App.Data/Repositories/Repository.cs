using App.Core.Entities;
using App.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Data.Repositories;

/// <summary>
/// Base implementation of the repository pattern for Entity Framework Core.
/// </summary>
/// <typeparam name="T">The entity type that inherits from Entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
public class Repository<T, TKey> : IRepository<T, TKey>
    where T : Entity<TKey>
{
    protected readonly AppDbContext _dbContext;

    public Repository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> AddAsync(T entity)
    {
        _dbContext.Set<T>().Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(TKey id)
    {
        var entity = await FindByIdAsync(id);
        if (entity == null)
            return false;

        _dbContext.Set<T>().Remove(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<T>> FindAllAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public async Task<T?> FindByIdAsync(TKey id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }
}
