using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;
using core.interfaces.dataaccess.repositories;
using dataaccess.converters.postgres;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;

using TicketDomain = core.domain.Ticket;
using TicketFilter = core.filters.TicketFilter;
using TicketPostgresqlModel = dataaccess.models.postgres.TicketPostgresqlModel;

namespace dataaccess.repositories.postgres;

/// <summary>
/// PostgreSQL implementation of ITicketRepository (per lab2-template v1 spec)
/// Table: ticket
/// Columns: ticket_uid, username, flight_number, price, status
/// </summary>
public class TicketPostgresqlRepository : ITicketRepository
{
    private readonly TicketsDatabaseContext _context;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="context">PostgreSQL database context</param>
    public TicketPostgresqlRepository(TicketsDatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a Ticket by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Ticket</param>
    /// <returns>The Ticket entity if found</returns>
    /// <exception cref="TicketNotFoundException">Thrown when Ticket with specified id is not found</exception>
    /// <exception cref="TicketDatabaseException">Thrown when database operation fails</exception>
    public async Task<TicketDomain> GetByIdAsync(int id)
    {
        try
        {
            var model = await _context.Ticket.FindAsync(id)
                ?? throw new TicketNotFoundException(id);

            return TicketPostgresqlConverter.ToDomain(model);
        }
        catch (TicketNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TicketDatabaseException($"Failed to get Ticket by id {id}", ex);
        }
    }

    /// <summary>
    /// Gets all Ticket entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying tickets (null returns all)</param>
    /// <returns>List of Ticket entities matching the filter or all tickets if filter is null</returns>
    /// <exception cref="TicketDatabaseException">Thrown when database operation fails</exception>
    public async Task<List<TicketDomain>> GetAllAsync(TicketFilter? filter = null)
    {
        try
        {
            var query = _context.Ticket.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            var models = await query.ToListAsync();
            return TicketPostgresqlConverter.ToDomainList(models);
        }
        catch (Exception ex)
        {
            throw new TicketDatabaseException("Failed to get all Tickets", ex);
        }
    }

    /// <summary>
    /// Creates a new Ticket
    /// </summary>
    /// <param name="ticket">The Ticket entity to create</param>
    /// <returns>The created Ticket entity with generated Id</returns>
    /// <exception cref="TicketAlreadyExistsException">Thrown when Ticket with same id already exists</exception>
    /// <exception cref="TicketDatabaseException">Thrown when database operation fails</exception>
    public async Task<TicketDomain> CreateAsync(TicketDomain ticket)
    {
        try
        {
            // Check if Ticket already exists
            if (await ExistsAsync(ticket.Id))
            {
                throw new TicketAlreadyExistsException(ticket.Id);
            }

            var model = TicketPostgresqlConverter.ToModel(ticket);
            _context.Ticket.Add(model);
            await _context.SaveChangesAsync();

            // Return the created model directly (it's already tracked with the generated ID)
            return TicketPostgresqlConverter.ToDomain(model);
        }
        catch (TicketAlreadyExistsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TicketDatabaseException("Failed to create Ticket", ex);
        }
    }

    /// <summary>
    /// Updates an existing Ticket
    /// </summary>
    /// <param name="ticket">The Ticket entity to update</param>
    /// <returns>The updated Ticket entity</returns>
    /// <exception cref="TicketNotFoundException">Thrown when Ticket with specified id is not found</exception>
    /// <exception cref="TicketDatabaseException">Thrown when database operation fails</exception>
    public async Task<TicketDomain> UpdateAsync(TicketDomain ticket)
    {
        try
        {
            var existingModel = await _context.Ticket.FindAsync(ticket.Id)
                ?? throw new TicketNotFoundException(ticket.Id);

            // Update properties (per spec: ticket_uid, username, flight_number, price, status)
            existingModel.TicketUid = ticket.TicketUid;
            existingModel.Username = ticket.Username;
            existingModel.FlightNumber = ticket.FlightNumber;
            existingModel.Price = ticket.Price;
            existingModel.Status = ticket.Status;

            await _context.SaveChangesAsync();

            return TicketPostgresqlConverter.ToDomain(existingModel);
        }
        catch (TicketNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TicketDatabaseException($"Failed to update Ticket with id {ticket.Id}", ex);
        }
    }

    /// <summary>
    /// Deletes a Ticket by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Ticket to delete</param>
    /// <returns>True if Ticket was deleted, false if not found</returns>
    /// <exception cref="TicketDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var model = await _context.Ticket.FindAsync(id);

            if (model == null)
            {
                return false;
            }

            _context.Ticket.Remove(model);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new TicketDatabaseException($"Failed to delete Ticket with id {id}", ex);
        }
    }

    /// <summary>
    /// Checks if a Ticket exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Ticket exists, false otherwise</returns>
    /// <exception cref="TicketDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _context.Ticket.AnyAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            throw new TicketDatabaseException($"Failed to check if Ticket with id {id} exists", ex);
        }
    }

    /// <summary>
    /// Gets the total count of Ticket entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting tickets (null counts all)</param>
    /// <returns>Total number of Ticket entities matching the filter</returns>
    /// <exception cref="TicketDatabaseException">Thrown when database operation fails</exception>
    public async Task<int> GetCountAsync(TicketFilter? filter = null)
    {
        try
        {
            var query = _context.Ticket.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            throw new TicketDatabaseException("Failed to get Tickets count", ex);
        }
    }

    /// <summary>
    /// Applies filter criteria to the query
    /// </summary>
    /// <param name="query">Base query</param>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Filtered query</returns>
    private IQueryable<TicketPostgresqlModel> ApplyFilter(
        IQueryable<TicketPostgresqlModel> query,
        TicketFilter filter)
    {
        if (filter == null)
            return query;

        // Filter by Username (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.Username))
        {
            var username = filter.Username.ToLower();
            query = query.Where(t => t.Username.ToLower().Contains(username));
        }

        // Filter by FlightNumber (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.FlightNumber))
        {
            var flightNumber = filter.FlightNumber.ToLower();
            query = query.Where(t => t.FlightNumber.ToLower().Contains(flightNumber));
        }

        // Filter by MinPrice
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(t => t.Price >= filter.MinPrice.Value);
        }

        // Filter by MaxPrice
        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(t => t.Price <= filter.MaxPrice.Value);
        }

        // Filter by Status
        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        return query;
    }
}
