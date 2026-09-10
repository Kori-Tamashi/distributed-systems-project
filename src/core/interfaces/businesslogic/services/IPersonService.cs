using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;

namespace core.interfaces.businesslogic.services;

/// <summary>
/// Service interface for Person business logic operations
/// Provides high-level operations for managing Person entities
/// </summary>
public interface IPersonService
{
    /// <summary>
    /// Gets a Person by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Person</param>
    /// <returns>The Person entity if found</returns>
    /// <exception cref="PersonNotFoundException">Thrown when Person with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Person> GetByIdAsync(int id);

    /// <summary>
    /// Gets all Person entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying persons (null returns all)</param>
    /// <returns>List of Person entities matching the filter or all persons if filter is null</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<List<Person>> GetAllAsync(PersonFilter? filter = null);

    /// <summary>
    /// Creates a new Person
    /// </summary>
    /// <param name="person">The Person entity to create</param>
    /// <returns>The created Person entity with generated Id</returns>
    /// <exception cref="PersonValidationException">Thrown when Person data is invalid</exception>
    /// <exception cref="PersonBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Person> CreateAsync(Person person);

    /// <summary>
    /// Updates an existing Person
    /// </summary>
    /// <param name="person">The Person entity to update</param>
    /// <returns>The updated Person entity</returns>
    /// <exception cref="PersonNotFoundException">Thrown when Person with specified id is not found</exception>
    /// <exception cref="PersonValidationException">Thrown when Person data is invalid</exception>
    /// <exception cref="PersonBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Person> UpdateAsync(Person person);

    /// <summary>
    /// Deletes a Person by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Person to delete</param>
    /// <returns>True if Person was deleted successfully</returns>
    /// <exception cref="PersonNotFoundException">Thrown when Person with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if a Person exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Person exists, false otherwise</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total count of Person entities
    /// </summary>
    /// <returns>Total number of Person entities</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<int> GetCountAsync();
}
