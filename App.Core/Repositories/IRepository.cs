namespace App.Core.Repositories;

/// <summary>
/// Generic repository interface for CRUD operations
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
/// <typeparam name="TKey">The type of the entity's key</typeparam>
public interface IRepository<T, TKey>
{
    /// <summary>
    /// Finds all entities
    /// </summary>
    /// <returns>An enumerable collection of entities</returns>
    Task<IEnumerable<T>> FindAllAsync();

    /// <summary>
    /// Finds an entity by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<T?> FindByIdAsync(TKey id);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to delete</param>
    /// <returns>True if the entity was deleted, otherwise false</returns>
    Task<bool> DeleteAsync(TKey id);
}
