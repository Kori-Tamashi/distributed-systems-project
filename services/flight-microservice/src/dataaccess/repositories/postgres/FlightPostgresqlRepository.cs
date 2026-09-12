using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;
using core.interfaces.dataaccess.repositories;
using dataaccess.converters.postgres;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;

using FlightDomain = core.domain.Flight;
using FlightFilter = core.filters.FlightFilter;
using FlightPostgresqlModel = dataaccess.models.postgres.FlightPostgresqlModel;

namespace dataaccess.repositories.postgres;

/// <summary>
/// PostgreSQL implementation of IFlightRepository
/// Provides data access operations for Flight entities
/// </summary>
public class FlightPostgresqlRepository : IFlightRepository
{
    private readonly FlightDatabaseContext _context;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="context">PostgreSQL database context</param>
    public FlightPostgresqlRepository(FlightDatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a Flight by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Flight</param>
    /// <returns>The Flight entity if found</returns>
    /// <exception cref="FlightNotFoundException">Thrown when Flight with specified id is not found</exception>
    /// <exception cref="FlightDatabaseException">Thrown when database operation fails</exception>
    public async Task<FlightDomain> GetByIdAsync(int id)
    {
        try
        {
            var model = await _context.Flights.FindAsync(id)
                ?? throw new FlightNotFoundException(id);

            return FlightPostgresqlConverter.ToDomain(model);
        }
        catch (FlightNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FlightDatabaseException($"Failed to get Flight by id {id}", ex);
        }
    }

    /// <summary>
    /// Gets all Flight entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying flights (null returns all)</param>
    /// <returns>List of Flight entities matching the filter or all flights if filter is null</returns>
    /// <exception cref="FlightDatabaseException">Thrown when database operation fails</exception>
    public async Task<List<FlightDomain>> GetAllAsync(FlightFilter? filter = null)
    {
        try
        {
            var query = _context.Flights.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            var models = await query.ToListAsync();
            return FlightPostgresqlConverter.ToDomainList(models);
        }
        catch (Exception ex)
        {
            throw new FlightDatabaseException("Failed to get all Flights", ex);
        }
    }

    /// <summary>
    /// Creates a new Flight
    /// </summary>
    /// <param name="flight">The Flight entity to create</param>
    /// <returns>The created Flight entity with generated Id</returns>
    /// <exception cref="FlightAlreadyExistsException">Thrown when Flight with same id already exists</exception>
    /// <exception cref="FlightDatabaseException">Thrown when database operation fails</exception>
    public async Task<FlightDomain> CreateAsync(FlightDomain flight)
    {
        try
        {
            // Check if Flight already exists
            if (await ExistsAsync(flight.Id))
            {
                throw new FlightAlreadyExistsException(flight.Id);
            }

            var model = FlightPostgresqlConverter.ToModel(flight);
            _context.Flights.Add(model);
            await _context.SaveChangesAsync();

            // Return updated entity with generated Id
            return await GetByIdAsync(model.Id);
        }
        catch (FlightAlreadyExistsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FlightDatabaseException("Failed to create Flight", ex);
        }
    }

    /// <summary>
    /// Updates an existing Flight
    /// </summary>
    /// <param name="flight">The Flight entity to update</param>
    /// <returns>The updated Flight entity</returns>
    /// <exception cref="FlightNotFoundException">Thrown when Flight with specified id is not found</exception>
    /// <exception cref="FlightDatabaseException">Thrown when database operation fails</exception>
    public async Task<FlightDomain> UpdateAsync(FlightDomain flight)
    {
        try
        {
            var existingModel = await _context.Flights.FindAsync(flight.Id)
                ?? throw new FlightNotFoundException(flight.Id);

            // Update properties
            existingModel.FlightUid = flight.FlightUid;
            existingModel.FlightNumber = flight.FlightNumber;
            existingModel.DateTime = flight.DateTime;
            existingModel.FromAirportId = flight.FromAirportId;
            existingModel.ToAirportId = flight.ToAirportId;
            existingModel.Price = flight.Price;

            await _context.SaveChangesAsync();

            return FlightPostgresqlConverter.ToDomain(existingModel);
        }
        catch (FlightNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FlightDatabaseException($"Failed to update Flight with id {flight.Id}", ex);
        }
    }

    /// <summary>
    /// Deletes a Flight by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Flight to delete</param>
    /// <returns>True if Flight was deleted, false if not found</returns>
    /// <exception cref="FlightDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var model = await _context.Flights.FindAsync(id);

            if (model == null)
            {
                return false;
            }

            _context.Flights.Remove(model);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new FlightDatabaseException($"Failed to delete Flight with id {id}", ex);
        }
    }

    /// <summary>
    /// Checks if a Flight exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Flight exists, false otherwise</returns>
    /// <exception cref="FlightDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _context.Flights.AnyAsync(f => f.Id == id);
        }
        catch (Exception ex)
        {
            throw new FlightDatabaseException($"Failed to check if Flight with id {id} exists", ex);
        }
    }

    /// <summary>
    /// Gets the total count of Flight entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting flights (null counts all)</param>
    /// <returns>Total number of Flight entities matching the filter</returns>
    /// <exception cref="FlightDatabaseException">Thrown when database operation fails</exception>
    public async Task<int> GetCountAsync(FlightFilter? filter = null)
    {
        try
        {
            var query = _context.Flights.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            throw new FlightDatabaseException("Failed to get Flights count", ex);
        }
    }

    /// <summary>
    /// Applies filter criteria to the query
    /// </summary>
    /// <param name="query">Base query</param>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Filtered query</returns>
    private IQueryable<FlightPostgresqlModel> ApplyFilter(
        IQueryable<FlightPostgresqlModel> query,
        FlightFilter filter)
    {
        if (filter == null)
            return query;

        // Filter by FlightNumber (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.FlightNumber))
        {
            var flightNumber = filter.FlightNumber.ToLower();
            query = query.Where(f => f.FlightNumber.ToLower().Contains(flightNumber));
        }

        // Filter by FromAirportId
        if (filter.FromAirportId.HasValue)
        {
            query = query.Where(f => f.FromAirportId == filter.FromAirportId.Value);
        }

        // Filter by ToAirportId
        if (filter.ToAirportId.HasValue)
        {
            query = query.Where(f => f.ToAirportId == filter.ToAirportId.Value);
        }

        // Filter by MinPrice
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(f => f.Price >= filter.MinPrice.Value);
        }

        // Filter by MaxPrice
        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(f => f.Price <= filter.MaxPrice.Value);
        }

        // Filter by MinDateTime
        if (filter.MinDateTime.HasValue)
        {
            query = query.Where(f => f.DateTime >= filter.MinDateTime.Value);
        }

        // Filter by MaxDateTime
        if (filter.MaxDateTime.HasValue)
        {
            query = query.Where(f => f.DateTime <= filter.MaxDateTime.Value);
        }

        return query;
    }
}
