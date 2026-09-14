using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;

using RepositoryBookingNotFoundException = core.exceptions.dataaccess.repositories.BookingNotFoundException;
using RepositoryBookingAlreadyExistsException = core.exceptions.dataaccess.repositories.BookingAlreadyExistsException;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Booking business logic operations
/// Provides high-level operations with validation and business rules
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;

    /// <summary>
    /// Initializes a new instance of BookingService
    /// </summary>
    /// <param name="bookingRepository">The Booking repository for data access</param>
    public BookingService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
    }

    /// <inheritdoc/>
    public async Task<Booking> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new BookingValidationException($"Invalid Booking ID: {id}. ID must be positive.");
        }

        try
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            return booking ?? throw new BookingNotFoundException(id);
        }
        catch (RepositoryBookingNotFoundException)
        {
            throw new BookingNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to get Booking with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<Booking>> GetAllAsync(BookingFilter? filter = null)
    {
        try
        {
            return await _bookingRepository.GetAllAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get all Bookings", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Booking> CreateAsync(Booking booking)
    {
        ValidateBooking(booking);

        try
        {
            var createdBooking = await _bookingRepository.CreateAsync(booking);
            return createdBooking;
        }
        catch (BookingValidationException)
        {
            throw;
        }
        catch (RepositoryBookingAlreadyExistsException)
        {
            throw new BookingBusinessRuleViolationException(
                "UniqueConstraint", 
                $"Booking with UID {booking.BookingUid} already exists");
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to create Booking", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Booking> UpdateAsync(Booking booking)
    {
        if (booking.Id <= 0)
        {
            throw new BookingValidationException($"Invalid Booking ID: {booking.Id}. ID must be positive.");
        }

        ValidateBooking(booking);

        // Check if booking exists before updating (outside try-catch)
        var exists = await _bookingRepository.ExistsAsync(booking.Id);
        if (!exists)
        {
            throw new BookingNotFoundException(booking.Id);
        }

        try
        {
            var updatedBooking = await _bookingRepository.UpdateAsync(booking);
            return updatedBooking;
        }
        catch (RepositoryBookingNotFoundException)
        {
            throw new BookingNotFoundException(booking.Id);
        }
        catch (BookingValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to update Booking with ID {booking.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new BookingValidationException($"Invalid Booking ID: {id}. ID must be positive.");
        }

        // Check if booking exists before deleting (outside try-catch)
        var exists = await _bookingRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new BookingNotFoundException(id);
        }

        try
        {
            return await _bookingRepository.DeleteAsync(id);
        }
        catch (RepositoryBookingNotFoundException)
        {
            throw new BookingNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to delete Booking with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(int id)
    {
        if (id <= 0)
        {
            throw new BookingValidationException($"Invalid Booking ID: {id}. ID must be positive.");
        }

        try
        {
            return await _bookingRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to check existence of Booking with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(BookingFilter? filter = null)
    {
        try
        {
            return await _bookingRepository.GetCountAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get Booking count", ex);
        }
    }

    /// <summary>
    /// Validates Booking entity for business rules
    /// </summary>
    /// <param name="booking">The Booking to validate</param>
    /// <exception cref="BookingValidationException">Thrown when validation fails</exception>
    private void ValidateBooking(Booking booking)
    {
        if (booking == null)
        {
            throw new BookingValidationException("Booking cannot be null");
        }

        var errors = new Dictionary<string, string[]>();

        // Validate CustomerName
        if (string.IsNullOrWhiteSpace(booking.CustomerName))
        {
            errors["CustomerName"] = new[] { "CustomerName is required and cannot be empty" };
        }
        else if (booking.CustomerName.Length > 255)
        {
            errors["CustomerName"] = new[] { "CustomerName cannot exceed 255 characters" };
        }

        // Validate CustomerEmail
        if (string.IsNullOrWhiteSpace(booking.CustomerEmail))
        {
            errors["CustomerEmail"] = new[] { "CustomerEmail is required and cannot be empty" };
        }
        else if (booking.CustomerEmail.Length > 255)
        {
            errors["CustomerEmail"] = new[] { "CustomerEmail cannot exceed 255 characters" };
        }

        // Validate CustomerPhone (optional but must be valid format if provided)
        if (!string.IsNullOrWhiteSpace(booking.CustomerPhone) && booking.CustomerPhone.Length > 50)
        {
            errors["CustomerPhone"] = new[] { "CustomerPhone cannot exceed 50 characters" };
        }

        // Validate BookingReference
        if (string.IsNullOrWhiteSpace(booking.BookingReference))
        {
            errors["BookingReference"] = new[] { "BookingReference is required and cannot be empty" };
        }
        else if (booking.BookingReference.Length > 50)
        {
            errors["BookingReference"] = new[] { "BookingReference cannot exceed 50 characters" };
        }

        // Validate PaymentTransactionId (optional but must be valid if provided)
        if (!string.IsNullOrWhiteSpace(booking.PaymentTransactionId) && booking.PaymentTransactionId.Length > 255)
        {
            errors["PaymentTransactionId"] = new[] { "PaymentTransactionId cannot exceed 255 characters" };
        }

        // Validate TotalPrice
        if (booking.TotalPrice <= 0)
        {
            errors["TotalPrice"] = new[] { "TotalPrice must be positive" };
        }
        else if (booking.TotalPrice > int.MaxValue)
        {
            errors["TotalPrice"] = new[] { "TotalPrice exceeds maximum allowed value" };
        }

        // Validate BookingDate (must be in the past or present)
        if (booking.BookingDate > DateTime.UtcNow)
        {
            errors["BookingDate"] = new[] { "Booking date and time cannot be in the future" };
        }

        if (errors.Count > 0)
        {
            var errorMessage = $"Booking validation failed with {errors.Count} error(s)";
            throw new BookingValidationException(errorMessage, errors);
        }
    }
}
