using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;

namespace core.interfaces.dataaccess.repositories;

/// <summary>
/// Repository interface for Airport entity operations
/// Provides CRUD operations for Airport entities
/// </summary>
public interface IAirportRepository
{
    /// <summary>
    /// Gets an Airport by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Airport</param>
    /// <returns>The Airport entity if found</returns>
    /// <exception cref="AirportNotFoundException">Thrown when Airport with specified id is not found</exception>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    Task<Airport> GetByIdAsync(int id);

    /// <summary>
    /// Gets all Airport entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying airports (null returns all)</param>
    /// <returns>List of all Airport entities matching the filter or all airports if filter is null</returns>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    Task<List<Airport>> GetAllAsync(AirportFilter? filter = null);

    /// <summary>
    /// Creates a new Airport entity
    /// </summary>
    /// <param name="airport">The Airport entity to create (Id will be ignored)</param>
    /// <returns>The created Airport entity with generated Id</returns>
    /// <exception cref="AirportAlreadyExistsException">Thrown when Airport with same id already exists</exception>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    Task<Airport> CreateAsync(Airport airport);

    /// <summary>
    /// Updates an existing Airport entity
    /// </summary>
    /// <param name="airport">The Airport entity to update</param>
    /// <returns>The updated Airport entity</returns>
    /// <exception cref="AirportNotFoundException">Thrown when Airport with specified id is not found</exception>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    Task<Airport> UpdateAsync(Airport airport);

    /// <summary>
    /// Deletes an Airport by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Airport to delete</param>
    /// <returns>True if Airport was deleted, false if not found</returns>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if an Airport exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Airport exists, false otherwise</returns>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total count of Airport entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting airports (null counts all)</param>
    /// <returns>Total number of Airport entities matching the filter</returns>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    Task<int> GetCountAsync(AirportFilter? filter = null);
}
