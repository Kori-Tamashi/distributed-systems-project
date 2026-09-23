using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;

namespace core.interfaces.dataaccess.repositories;

/// <summary>
/// Repository interface for Privilege entity operations
/// Provides CRUD operations and filtering for Privilege entities
/// </summary>
public interface IPrivilegeRepository
{
    /// <summary>
    /// Gets a Privilege by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Privilege</param>
    /// <returns>The Privilege entity if found</returns>
    /// <exception cref="PrivilegeNotFoundException">Thrown when Privilege with specified id is not found</exception>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    Task<Privilege> GetByIdAsync(int id);

    /// <summary>
    /// Gets all Privilege entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying privileges (null returns all)</param>
    /// <returns>List of Privilege entities matching the filter or all privileges if filter is null</returns>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    Task<List<Privilege>> GetAllAsync(PrivilegeFilter? filter = null);

    /// <summary>
    /// Creates a new Privilege entity
    /// </summary>
    /// <param name="privilege">The Privilege entity to create (Id will be ignored)</param>
    /// <returns>The created Privilege entity with generated Id</returns>
    /// <exception cref="PrivilegeAlreadyExistsException">Thrown when Privilege with same id already exists</exception>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    Task<Privilege> CreateAsync(Privilege privilege);

    /// <summary>
    /// Updates an existing Privilege entity
    /// </summary>
    /// <param name="privilege">The Privilege entity to update</param>
    /// <returns>The updated Privilege entity</returns>
    /// <exception cref="PrivilegeNotFoundException">Thrown when Privilege with specified id is not found</exception>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    Task<Privilege> UpdateAsync(Privilege privilege);

    /// <summary>
    /// Deletes a Privilege by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Privilege to delete</param>
    /// <returns>True if Privilege was deleted, false if not found</returns>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if a Privilege exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Privilege exists, false otherwise</returns>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total count of Privilege entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting privileges (null counts all)</param>
    /// <returns>Total number of Privilege entities matching the filter</returns>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    Task<int> GetCountAsync(PrivilegeFilter? filter = null);
}
