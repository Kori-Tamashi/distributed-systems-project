using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;

namespace core.interfaces.dataaccess.repositories;

/// <summary>
/// Repository interface for Person entity operations
/// Provides CRUD operations and filtering for Person entities
/// </summary>
public interface IPersonRepository
{
    /// <summary>
    /// Gets a Person by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Person</param>
    /// <returns>The Person entity if found</returns>
    /// <exception cref="PersonNotFoundException">Thrown when Person with specified id is not found</exception>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    Task<Person> GetByIdAsync(int id);

    /// <summary>
    /// Gets all Person entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying persons (null returns all)</param>
    /// <returns>List of Person entities matching the filter or all persons if filter is null</returns>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    Task<List<Person>> GetAllAsync(PersonFilter? filter = null);

    /// <summary>
    /// Creates a new Person entity
    /// </summary>
    /// <param name="person">The Person entity to create (Id will be ignored)</param>
    /// <returns>The created Person entity with generated Id</returns>
    /// <exception cref="PersonAlreadyExistsException">Thrown when Person with same id already exists</exception>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    Task<Person> CreateAsync(Person person);

    /// <summary>
    /// Updates an existing Person entity
    /// </summary>
    /// <param name="person">The Person entity to update</param>
    /// <returns>The updated Person entity</returns>
    /// <exception cref="PersonNotFoundException">Thrown when Person with specified id is not found</exception>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    Task<Person> UpdateAsync(Person person);

    /// <summary>
    /// Deletes a Person by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Person to delete</param>
    /// <returns>True if Person was deleted, false if not found</returns>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if a Person exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Person exists, false otherwise</returns>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total count of Person entities
    /// </summary>
    /// <returns>Total number of Person entities</returns>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    Task<int> GetCountAsync();
}
