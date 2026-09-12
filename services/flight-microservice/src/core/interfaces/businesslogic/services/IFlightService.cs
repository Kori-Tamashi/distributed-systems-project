using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;

namespace core.interfaces.businesslogic.services;

/// <summary>
/// Service interface for Flight business logic operations
/// Provides high-level operations for managing Flight entities
/// </summary>
public interface IFlightService
{
    /// <summary>
    /// Gets a Flight by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Flight</param>
    /// <returns>The Flight entity if found</returns>
    /// <exception cref="FlightNotFoundException">Thrown when Flight with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Flight> GetByIdAsync(int id);

    /// <summary>
    /// Gets all Flight entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying flights (null returns all)</param>
    /// <returns>List of Flight entities matching the filter or all flights if filter is null</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<List<Flight>> GetAllAsync(FlightFilter? filter = null);

    /// <summary>
    /// Creates a new Flight
    /// </summary>
    /// <param name="flight">The Flight entity to create</param>
    /// <returns>The created Flight entity with generated Id</returns>
    /// <exception cref="FlightValidationException">Thrown when Flight data is invalid</exception>
    /// <exception cref="FlightBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Flight> CreateAsync(Flight flight);

    /// <summary>
    /// Updates an existing Flight
    /// </summary>
    /// <param name="flight">The Flight entity to update</param>
    /// <returns>The updated Flight entity</returns>
    /// <exception cref="FlightNotFoundException">Thrown when Flight with specified id is not found</exception>
    /// <exception cref="FlightValidationException">Thrown when Flight data is invalid</exception>
    /// <exception cref="FlightBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Flight> UpdateAsync(Flight flight);

    /// <summary>
    /// Deletes a Flight by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Flight to delete</param>
    /// <returns>True if Flight was deleted successfully</returns>
    /// <exception cref="FlightNotFoundException">Thrown when Flight with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if a Flight exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Flight exists, false otherwise</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total count of Flight entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting flights (null counts all)</param>
    /// <returns>Total number of Flight entities matching the filter</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<int> GetCountAsync(FlightFilter? filter = null);
}
