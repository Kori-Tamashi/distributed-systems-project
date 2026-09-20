using core.domain;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using Microsoft.Extensions.Logging;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Booking business logic operations
/// Provides high-level operations with validation, business rules, and error handling
/// Uses HTTP Gateway to communicate with Ticket microservice
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingGateway _bookingGateway;
    private readonly ILogger<BookingService> _logger;

    /// <summary>
    /// Initializes a new instance of BookingService
    /// </summary>
    /// <param name="bookingGateway">The Booking Gateway for HTTP communication</param>
    /// <param name="logger">Logger for SAGA tracking</param>
    public BookingService(IBookingGateway bookingGateway, ILogger<BookingService> logger)
    {
        _bookingGateway = bookingGateway ?? throw new ArgumentNullException(nameof(bookingGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            var booking = await _bookingGateway.GetByIdAsync(id);
            if (booking == null)
            {
                throw new BookingNotFoundException(id);
            }
            return booking;
        }
        catch (BookingNotFoundException)
        {
            throw;
        }
        catch (BookingGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Ticket microservice for GetByIdAsync");
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Booking with ID {Id}", id);
            throw new ValidationException($"Failed to get Booking with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<Booking>> GetAllAsync(BookingFilter? filter = null)
    {
        try
        {
            var bookings = filter != null 
                ? await _bookingGateway.GetAllAsync(filter)
                : await _bookingGateway.GetAllAsync();
            return bookings.ToList();
        }
        catch (BookingGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Ticket microservice for GetAllAsync");
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all Bookings");
            throw new ValidationException("Failed to get all Bookings", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Booking> CreateAsync(Booking booking)
    {
        ValidateBooking(booking);

        try
        {
            var createdBooking = await _bookingGateway.CreateAsync(booking);
            _logger.LogInformation("Booking created with ID {Id}", createdBooking.Id);
            return createdBooking;
        }
        catch (BookingValidationException)
        {
            _logger.LogWarning("Booking validation failed");
            throw;
        }
        catch (BookingGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to create Booking");
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Booking");
            throw new ValidationException("Failed to create Booking", ex);
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

        try
        {
            var updatedBooking = await _bookingGateway.UpdateAsync(booking);
            _logger.LogInformation("Booking {Id} updated successfully", booking.Id);
            return updatedBooking;
        }
        catch (BookingGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Booking {Id} not found", booking.Id);
            throw new BookingNotFoundException(booking.Id);
        }
        catch (BookingValidationException)
        {
            _logger.LogWarning("Booking validation failed");
            throw;
        }
        catch (BookingGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to update Booking {Id}", booking.Id);
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Booking {Id}", booking.Id);
            throw new ValidationException($"Failed to update Booking with ID {booking.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new BookingValidationException($"Invalid Booking ID: {id}. ID must be positive.");
        }

        try
        {
            await _bookingGateway.DeleteAsync(id);
            _logger.LogInformation("Booking {Id} deleted successfully", id);
        }
        catch (BookingNotFoundException)
        {
            throw;
        }
        catch (BookingGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Booking {Id} not found", id);
            throw new BookingNotFoundException(id);
        }
        catch (BookingGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to delete Booking {Id}", id);
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Booking {Id}", id);
            throw new ValidationException($"Failed to delete Booking with ID {id}", ex);
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
            var booking = await _bookingGateway.GetByIdAsync(id);
            return booking != null;
        }
        catch (BookingGatewayEntityNotFoundException)
        {
            return false;
        }
        catch (BookingGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Ticket microservice for ExistsAsync");
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence of Booking with ID {Id}", id);
            throw new ValidationException($"Failed to check existence of Booking with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(BookingFilter? filter = null)
    {
        try
        {
            var bookings = filter != null 
                ? await _bookingGateway.GetAllAsync(filter)
                : await _bookingGateway.GetAllAsync();
            return bookings.Count();
        }
        catch (BookingGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Ticket microservice for GetCountAsync");
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Booking count");
            throw new ValidationException("Failed to get Booking count", ex);
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
