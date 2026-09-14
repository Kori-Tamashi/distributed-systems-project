using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;
using core.interfaces.dataaccess.repositories;
using dataaccess.converters.postgres;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;

using BookingDomain = core.domain.Booking;
using BookingFilter = core.filters.BookingFilter;
using BookingPostgresqlModel = dataaccess.models.postgres.BookingPostgresqlModel;

namespace dataaccess.repositories.postgres;

/// <summary>
/// PostgreSQL implementation of IBookingRepository
/// Provides data access operations for Booking entities
/// </summary>
public class BookingPostgresqlRepository : IBookingRepository
{
    private readonly TicketsDatabaseContext _context;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="context">PostgreSQL database context</param>
    public BookingPostgresqlRepository(TicketsDatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a Booking by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Booking</param>
    /// <returns>The Booking entity if found</returns>
    /// <exception cref="BookingNotFoundException">Thrown when Booking with specified id is not found</exception>
    /// <exception cref="BookingDatabaseException">Thrown when database operation fails</exception>
    public async Task<BookingDomain> GetByIdAsync(int id)
    {
        try
        {
            var model = await _context.Bookings.FindAsync(id)
                ?? throw new BookingNotFoundException(id);

            return BookingPostgresqlConverter.ToDomain(model);
        }
        catch (BookingNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BookingDatabaseException($"Failed to get Booking by id {id}", ex);
        }
    }

    /// <summary>
    /// Gets all Booking entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying bookings (null returns all)</param>
    /// <returns>List of all Booking entities matching the filter or all bookings if filter is null</returns>
    /// <exception cref="BookingDatabaseException">Thrown when database operation fails</exception>
    public async Task<List<BookingDomain>> GetAllAsync(BookingFilter? filter = null)
    {
        try
        {
            var query = _context.Bookings.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            var models = await query.ToListAsync();
            return BookingPostgresqlConverter.ToDomainList(models);
        }
        catch (Exception ex)
        {
            throw new BookingDatabaseException("Failed to get all Bookings", ex);
        }
    }

    /// <summary>
    /// Creates a new Booking
    /// </summary>
    /// <param name="booking">The Booking entity to create</param>
    /// <returns>The created Booking entity with generated Id</returns>
    /// <exception cref="BookingAlreadyExistsException">Thrown when Booking with same id already exists</exception>
    /// <exception cref="BookingDatabaseException">Thrown when database operation fails</exception>
    public async Task<BookingDomain> CreateAsync(BookingDomain booking)
    {
        try
        {
            // Check if Booking already exists
            if (await ExistsAsync(booking.Id))
            {
                throw new BookingAlreadyExistsException(booking.Id);
            }

            var model = BookingPostgresqlConverter.ToModel(booking);
            _context.Bookings.Add(model);
            await _context.SaveChangesAsync();

            // Return updated entity with generated Id
            return await GetByIdAsync(model.Id);
        }
        catch (BookingAlreadyExistsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BookingDatabaseException("Failed to create Booking", ex);
        }
    }

    /// <summary>
    /// Updates an existing Booking
    /// </summary>
    /// <param name="booking">The Booking entity to update</param>
    /// <returns>The updated Booking entity</returns>
    /// <exception cref="BookingNotFoundException">Thrown when Booking with specified id is not found</exception>
    /// <exception cref="BookingDatabaseException">Thrown when database operation fails</exception>
    public async Task<BookingDomain> UpdateAsync(BookingDomain booking)
    {
        try
        {
            var existingModel = await _context.Bookings.FindAsync(booking.Id)
                ?? throw new BookingNotFoundException(booking.Id);

            // Update properties
            existingModel.BookingUid = booking.BookingUid;
            existingModel.BookingReference = booking.BookingReference;
            existingModel.CustomerName = booking.CustomerName;
            existingModel.CustomerEmail = booking.CustomerEmail;
            existingModel.CustomerPhone = booking.CustomerPhone;
            existingModel.TotalPrice = booking.TotalPrice;
            existingModel.BookingDate = booking.BookingDate;
            existingModel.Status = (int)booking.Status;
            existingModel.PaymentMethod = (int)booking.PaymentMethod;
            existingModel.PaymentTransactionId = booking.PaymentTransactionId;

            await _context.SaveChangesAsync();

            return BookingPostgresqlConverter.ToDomain(existingModel);
        }
        catch (BookingNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BookingDatabaseException($"Failed to update Booking with id {booking.Id}", ex);
        }
    }

    /// <summary>
    /// Deletes a Booking by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Booking to delete</param>
    /// <returns>True if Booking was deleted, false if not found</returns>
    /// <exception cref="BookingDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var model = await _context.Bookings.FindAsync(id);

            if (model == null)
            {
                return false;
            }

            _context.Bookings.Remove(model);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new BookingDatabaseException($"Failed to delete Booking with id {id}", ex);
        }
    }

    /// <summary>
    /// Checks if a Booking exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Booking exists, false otherwise</returns>
    /// <exception cref="BookingDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _context.Bookings.AnyAsync(b => b.Id == id);
        }
        catch (Exception ex)
        {
            throw new BookingDatabaseException($"Failed to check if Booking with id {id} exists", ex);
        }
    }

    /// <summary>
    /// Gets the total count of Booking entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting bookings (null counts all)</param>
    /// <returns>Total number of Booking entities matching the filter</returns>
    /// <exception cref="BookingDatabaseException">Thrown when database operation fails</exception>
    public async Task<int> GetCountAsync(BookingFilter? filter = null)
    {
        try
        {
            var query = _context.Bookings.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            throw new BookingDatabaseException("Failed to get Bookings count", ex);
        }
    }

    /// <summary>
    /// Applies filter criteria to the query
    /// </summary>
    /// <param name="query">Base query</param>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Filtered query</returns>
    private IQueryable<BookingPostgresqlModel> ApplyFilter(
        IQueryable<BookingPostgresqlModel> query,
        BookingFilter filter)
    {
        if (filter == null)
            return query;

        // Filter by BookingReference (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.BookingReference))
        {
            var bookingReference = filter.BookingReference.ToLower();
            query = query.Where(b => b.BookingReference.ToLower().Contains(bookingReference));
        }

        // Filter by CustomerName (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.CustomerName))
        {
            var customerName = filter.CustomerName.ToLower();
            query = query.Where(b => b.CustomerName.ToLower().Contains(customerName));
        }

        // Filter by CustomerEmail (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.CustomerEmail))
        {
            var customerEmail = filter.CustomerEmail.ToLower();
            query = query.Where(b => b.CustomerEmail.ToLower().Contains(customerEmail));
        }

        // Filter by MinTotalPrice
        if (filter.MinTotalPrice.HasValue)
        {
            query = query.Where(b => b.TotalPrice >= filter.MinTotalPrice.Value);
        }

        // Filter by MaxTotalPrice
        if (filter.MaxTotalPrice.HasValue)
        {
            query = query.Where(b => b.TotalPrice <= filter.MaxTotalPrice.Value);
        }

        // Filter by MinBookingDate
        if (filter.MinBookingDate.HasValue)
        {
            query = query.Where(b => b.BookingDate >= filter.MinBookingDate.Value);
        }

        // Filter by MaxBookingDate
        if (filter.MaxBookingDate.HasValue)
        {
            query = query.Where(b => b.BookingDate <= filter.MaxBookingDate.Value);
        }

        // Filter by Status
        if (filter.Status.HasValue)
        {
            query = query.Where(b => b.Status == (int)filter.Status.Value);
        }

        // Filter by PaymentMethod
        if (filter.PaymentMethod.HasValue)
        {
            query = query.Where(b => b.PaymentMethod == (int)filter.PaymentMethod.Value);
        }

        return query;
    }
}
