using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;

namespace core.interfaces.dataaccess.repositories;

/// <summary>
/// Repository interface for PrivilegeHistory entity operations
/// Provides CRUD operations and filtering for PrivilegeHistory entities
/// </summary>
public interface IPrivilegeHistoryRepository
{
    /// <summary>
    /// Gets a PrivilegeHistory by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the PrivilegeHistory</param>
    /// <returns>The PrivilegeHistory entity if found</returns>
    /// <exception cref="PrivilegeHistoryNotFoundException">Thrown when PrivilegeHistory with specified id is not found</exception>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    Task<PrivilegeHistory> GetByIdAsync(int id);

    /// <summary>
    /// Gets all PrivilegeHistory entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying history (null returns all)</param>
    /// <returns>List of PrivilegeHistory entities matching the filter or all history if filter is null</returns>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    Task<List<PrivilegeHistory>> GetAllAsync(PrivilegeHistoryFilter? filter = null);

    /// <summary>
    /// Creates a new PrivilegeHistory entity
    /// </summary>
    /// <param name="history">The PrivilegeHistory entity to create (Id will be ignored)</param>
    /// <returns>The created PrivilegeHistory entity with generated Id</returns>
    /// <exception cref="PrivilegeHistoryAlreadyExistsException">Thrown when PrivilegeHistory with same id already exists</exception>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    Task<PrivilegeHistory> CreateAsync(PrivilegeHistory history);

    /// <summary>
    /// Updates an existing PrivilegeHistory entity
    /// </summary>
    /// <param name="history">The PrivilegeHistory entity to update</param>
    /// <returns>The updated PrivilegeHistory entity</returns>
    /// <exception cref="PrivilegeHistoryNotFoundException">Thrown when PrivilegeHistory with specified id is not found</exception>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    Task<PrivilegeHistory> UpdateAsync(PrivilegeHistory history);

    /// <summary>
    /// Deletes a PrivilegeHistory by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the PrivilegeHistory to delete</param>
    /// <returns>True if PrivilegeHistory was deleted, false if not found</returns>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if a PrivilegeHistory exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if PrivilegeHistory exists, false otherwise</returns>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total count of PrivilegeHistory entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting history (null counts all)</param>
    /// <returns>Total number of PrivilegeHistory entities matching the filter</returns>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    Task<int> GetCountAsync(PrivilegeHistoryFilter? filter = null);
}
