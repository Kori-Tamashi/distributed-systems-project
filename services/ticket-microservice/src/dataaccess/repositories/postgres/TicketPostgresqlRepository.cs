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
/// PostgreSQL implementation of ITicketRepository
/// Provides data access operations for Ticket entities
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
            var model = await _context.Tickets.FindAsync(id)
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
            var query = _context.Tickets.AsQueryable();

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
            _context.Tickets.Add(model);
            await _context.SaveChangesAsync();

            // Return updated entity with generated Id
            return await GetByIdAsync(model.Id);
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
            var existingModel = await _context.Tickets.FindAsync(ticket.Id)
                ?? throw new TicketNotFoundException(ticket.Id);

            // Update properties
            existingModel.TicketUid = ticket.TicketUid;
            existingModel.FlightId = ticket.FlightId;
            existingModel.PassengerName = ticket.PassengerName;
            existingModel.PassengerEmail = ticket.PassengerEmail;
            existingModel.PassengerPhone = ticket.PassengerPhone;
            existingModel.SeatNumber = ticket.SeatNumber;
            existingModel.Class = (int)ticket.Class;
            existingModel.Price = ticket.Price;
            existingModel.BookingDate = ticket.BookingDate;
            existingModel.Status = (int)ticket.Status;

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
            var model = await _context.Tickets.FindAsync(id);

            if (model == null)
            {
                return false;
            }

            _context.Tickets.Remove(model);
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
            return await _context.Tickets.AnyAsync(t => t.Id == id);
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
            var query = _context.Tickets.AsQueryable();

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

        // Filter by FlightId
        if (filter.FlightId.HasValue)
        {
            query = query.Where(t => t.FlightId == filter.FlightId.Value);
        }

        // Filter by PassengerName (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.PassengerName))
        {
            var passengerName = filter.PassengerName.ToLower();
            query = query.Where(t => t.PassengerName.ToLower().Contains(passengerName));
        }

        // Filter by PassengerEmail (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.PassengerEmail))
        {
            var passengerEmail = filter.PassengerEmail.ToLower();
            query = query.Where(t => t.PassengerEmail.ToLower().Contains(passengerEmail));
        }

        // Filter by SeatNumber (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.SeatNumber))
        {
            var seatNumber = filter.SeatNumber.ToLower();
            query = query.Where(t => t.SeatNumber.ToLower().Contains(seatNumber));
        }

        // Filter by Class
        if (filter.Class.HasValue)
        {
            query = query.Where(t => t.Class == (int)filter.Class.Value);
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

        // Filter by MinBookingDate
        if (filter.MinBookingDate.HasValue)
        {
            query = query.Where(t => t.BookingDate >= filter.MinBookingDate.Value);
        }

        // Filter by MaxBookingDate
        if (filter.MaxBookingDate.HasValue)
        {
            query = query.Where(t => t.BookingDate <= filter.MaxBookingDate.Value);
        }

        // Filter by Status
        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == (int)filter.Status.Value);
        }

        return query;
    }
}
