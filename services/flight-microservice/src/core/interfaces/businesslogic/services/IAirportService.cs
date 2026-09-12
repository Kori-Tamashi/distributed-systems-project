using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;

namespace core.interfaces.businesslogic.services;

/// <summary>
/// Service interface for Airport business logic operations
/// Provides high-level operations for managing Airport entities
/// </summary>
public interface IAirportService
{
    /// <summary>
    /// Gets an Airport by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Airport</param>
    /// <returns>The Airport entity if found</returns>
    /// <exception cref="AirportNotFoundException">Thrown when Airport with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Airport> GetByIdAsync(int id);

    /// <summary>
    /// Gets all Airport entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying airports (null returns all)</param>
    /// <returns>List of all Airport entities matching the filter or all airports if filter is null</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<List<Airport>> GetAllAsync(AirportFilter? filter = null);

    /// <summary>
    /// Creates a new Airport
    /// </summary>
    /// <param name="airport">The Airport entity to create</param>
    /// <returns>The created Airport entity with generated Id</returns>
    /// <exception cref="AirportValidationException">Thrown when Airport data is invalid</exception>
    /// <exception cref="AirportBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Airport> CreateAsync(Airport airport);

    /// <summary>
    /// Updates an existing Airport
    /// </summary>
    /// <param name="airport">The Airport entity to update</param>
    /// <returns>The updated Airport entity</returns>
    /// <exception cref="AirportNotFoundException">Thrown when Airport with specified id is not found</exception>
    /// <exception cref="AirportValidationException">Thrown when Airport data is invalid</exception>
    /// <exception cref="AirportBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Airport> UpdateAsync(Airport airport);

    /// <summary>
    /// Deletes an Airport by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Airport to delete</param>
    /// <returns>True if Airport was deleted successfully</returns>
    /// <exception cref="AirportNotFoundException">Thrown when Airport with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if an Airport exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Airport exists, false otherwise</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total count of Airport entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting airports (null counts all)</param>
    /// <returns>Total number of Airport entities matching the filter</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<int> GetCountAsync(AirportFilter? filter = null);
}
