using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;
using core.interfaces.dataaccess.repositories;
using dataaccess.converters.postgres;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;

using AirportDomain = core.domain.Airport;
using AirportFilter = core.filters.AirportFilter;
using AirportPostgresqlModel = dataaccess.models.postgres.AirportPostgresqlModel;

namespace dataaccess.repositories.postgres;

/// <summary>
/// PostgreSQL implementation of IAirportRepository
/// Provides data access operations for Airport entities
/// </summary>
public class AirportPostgresqlRepository : IAirportRepository
{
    private readonly FlightDatabaseContext _context;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="context">PostgreSQL database context</param>
    public AirportPostgresqlRepository(FlightDatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets an Airport by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Airport</param>
    /// <returns>The Airport entity if found</returns>
    /// <exception cref="AirportNotFoundException">Thrown when Airport with specified id is not found</exception>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    public async Task<AirportDomain> GetByIdAsync(int id)
    {
        try
        {
            var model = await _context.Airports.FindAsync(id)
                ?? throw new AirportNotFoundException(id);

            return AirportPostgresqlConverter.ToDomain(model);
        }
        catch (AirportNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AirportDatabaseException($"Failed to get Airport by id {id}", ex);
        }
    }

    /// <summary>
    /// Gets all Airport entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying airports (null returns all)</param>
    /// <returns>List of all Airport entities matching the filter or all airports if filter is null</returns>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    public async Task<List<AirportDomain>> GetAllAsync(AirportFilter? filter = null)
    {
        try
        {
            var query = _context.Airports.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            var models = await query.ToListAsync();
            return AirportPostgresqlConverter.ToDomainList(models);
        }
        catch (Exception ex)
        {
            throw new AirportDatabaseException("Failed to get all Airports", ex);
        }
    }

    /// <summary>
    /// Creates a new Airport
    /// </summary>
    /// <param name="airport">The Airport entity to create</param>
    /// <returns>The created Airport entity with generated Id</returns>
    /// <exception cref="AirportAlreadyExistsException">Thrown when Airport with same id already exists</exception>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    public async Task<AirportDomain> CreateAsync(AirportDomain airport)
    {
        try
        {
            // Check if Airport already exists
            if (await ExistsAsync(airport.Id))
            {
                throw new AirportAlreadyExistsException(airport.Id);
            }

            var model = AirportPostgresqlConverter.ToModel(airport);
            _context.Airports.Add(model);
            await _context.SaveChangesAsync();

            // Return updated entity with generated Id
            return await GetByIdAsync(model.Id);
        }
        catch (AirportAlreadyExistsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AirportDatabaseException("Failed to create Airport", ex);
        }
    }

    /// <summary>
    /// Updates an existing Airport
    /// </summary>
    /// <param name="airport">The Airport entity to update</param>
    /// <returns>The updated Airport entity</returns>
    /// <exception cref="AirportNotFoundException">Thrown when Airport with specified id is not found</exception>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    public async Task<AirportDomain> UpdateAsync(AirportDomain airport)
    {
        try
        {
            var existingModel = await _context.Airports.FindAsync(airport.Id)
                ?? throw new AirportNotFoundException(airport.Id);

            // Update properties
            existingModel.Name = airport.Name;
            existingModel.City = airport.City;
            existingModel.Country = airport.Country;

            await _context.SaveChangesAsync();

            return AirportPostgresqlConverter.ToDomain(existingModel);
        }
        catch (AirportNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AirportDatabaseException($"Failed to update Airport with id {airport.Id}", ex);
        }
    }

    /// <summary>
    /// Deletes an Airport by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Airport to delete</param>
    /// <returns>True if Airport was deleted, false if not found</returns>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var model = await _context.Airports.FindAsync(id);

            if (model == null)
            {
                return false;
            }

            _context.Airports.Remove(model);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new AirportDatabaseException($"Failed to delete Airport with id {id}", ex);
        }
    }

    /// <summary>
    /// Checks if an Airport exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Airport exists, false otherwise</returns>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _context.Airports.AnyAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            throw new AirportDatabaseException($"Failed to check if Airport with id {id} exists", ex);
        }
    }

    /// <summary>
    /// Gets the total count of Airport entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting airports (null counts all)</param>
    /// <returns>Total number of Airport entities matching the filter</returns>
    /// <exception cref="AirportDatabaseException">Thrown when database operation fails</exception>
    public async Task<int> GetCountAsync(AirportFilter? filter = null)
    {
        try
        {
            var query = _context.Airports.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            throw new AirportDatabaseException("Failed to get Airports count", ex);
        }
    }

    /// <summary>
    /// Applies filter criteria to the query
    /// </summary>
    /// <param name="query">Base query</param>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Filtered query</returns>
    private IQueryable<AirportPostgresqlModel> ApplyFilter(
        IQueryable<AirportPostgresqlModel> query,
        AirportFilter filter)
    {
        if (filter == null)
            return query;

        // Filter by Name (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.Name))
        {
            var name = filter.Name.ToLower();
            query = query.Where(a => a.Name.ToLower().Contains(name));
        }

        // Filter by City (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.City))
        {
            var city = filter.City.ToLower();
            query = query.Where(a => a.City.ToLower().Contains(city));
        }

        // Filter by Country (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.Country))
        {
            var country = filter.Country.ToLower();
            query = query.Where(a => a.Country.ToLower().Contains(country));
        }

        return query;
    }
}
