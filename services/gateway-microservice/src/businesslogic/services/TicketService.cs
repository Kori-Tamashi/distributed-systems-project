using core.domain;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using Microsoft.Extensions.Logging;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Ticket business logic operations
/// Provides high-level operations with validation, business rules, and error handling
/// Uses HTTP Gateway to communicate with Ticket microservice
/// </summary>
public class TicketService : ITicketService
{
    private readonly ITicketGateway _ticketGateway;
    private readonly ILogger<TicketService> _logger;

    /// <summary>
    /// Initializes a new instance of TicketService
    /// </summary>
    /// <param name="ticketGateway">The Ticket Gateway for HTTP communication</param>
    /// <param name="logger">Logger for SAGA tracking</param>
    public TicketService(ITicketGateway ticketGateway, ILogger<TicketService> logger)
    {
        _ticketGateway = ticketGateway ?? throw new ArgumentNullException(nameof(ticketGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Ticket> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new TicketValidationException($"Invalid Ticket ID: {id}. ID must be positive.");
        }

        try
        {
            var ticket = await _ticketGateway.GetByIdAsync(id);
            if (ticket == null)
            {
                throw new TicketNotFoundException(id);
            }
            return ticket;
        }
        catch (TicketNotFoundException)
        {
            throw;
        }
        catch (TicketGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Ticket microservice for GetByIdAsync");
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Ticket with ID {Id}", id);
            throw new ValidationException($"Failed to get Ticket with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<Ticket>> GetAllAsync(TicketFilter? filter = null)
    {
        try
        {
            var tickets = filter != null 
                ? await _ticketGateway.GetAllAsync(filter)
                : await _ticketGateway.GetAllAsync();
            return tickets.ToList();
        }
        catch (TicketGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Ticket microservice for GetAllAsync");
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all Tickets");
            throw new ValidationException("Failed to get all Tickets", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Ticket> CreateAsync(Ticket ticket)
    {
        ValidateTicket(ticket);

        try
        {
            var createdTicket = await _ticketGateway.CreateAsync(ticket);
            _logger.LogInformation("Ticket created with ID {Id}", createdTicket.Id);
            return createdTicket;
        }
        catch (TicketValidationException)
        {
            _logger.LogWarning("Ticket validation failed");
            throw;
        }
        catch (TicketGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to create Ticket");
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Ticket");
            throw new ValidationException("Failed to create Ticket", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Ticket> UpdateAsync(Ticket ticket)
    {
        if (ticket.Id <= 0)
        {
            throw new TicketValidationException($"Invalid Ticket ID: {ticket.Id}. ID must be positive.");
        }

        ValidateTicket(ticket);

        try
        {
            var updatedTicket = await _ticketGateway.UpdateAsync(ticket);
            _logger.LogInformation("Ticket {Id} updated successfully", ticket.Id);
            return updatedTicket;
        }
        catch (TicketGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Ticket {Id} not found", ticket.Id);
            throw new TicketNotFoundException(ticket.Id);
        }
        catch (TicketValidationException)
        {
            _logger.LogWarning("Ticket validation failed");
            throw;
        }
        catch (TicketGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to update Ticket {Id}", ticket.Id);
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Ticket {Id}", ticket.Id);
            throw new ValidationException($"Failed to update Ticket with ID {ticket.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new TicketValidationException($"Invalid Ticket ID: {id}. ID must be positive.");
        }

        try
        {
            await _ticketGateway.DeleteAsync(id);
            _logger.LogInformation("Ticket {Id} deleted successfully", id);
        }
        catch (TicketNotFoundException)
        {
            throw;
        }
        catch (TicketGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Ticket {Id} not found", id);
            throw new TicketNotFoundException(id);
        }
        catch (TicketGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to delete Ticket {Id}", id);
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Ticket {Id}", id);
            throw new ValidationException($"Failed to delete Ticket with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(int id)
    {
        if (id <= 0)
        {
            throw new TicketValidationException($"Invalid Ticket ID: {id}. ID must be positive.");
        }

        try
        {
            var ticket = await _ticketGateway.GetByIdAsync(id);
            return ticket != null;
        }
        catch (TicketGatewayEntityNotFoundException)
        {
            return false;
        }
        catch (TicketGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Ticket microservice for ExistsAsync");
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence of Ticket with ID {Id}", id);
            throw new ValidationException($"Failed to check existence of Ticket with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(TicketFilter? filter = null)
    {
        try
        {
            var tickets = filter != null 
                ? await _ticketGateway.GetAllAsync(filter)
                : await _ticketGateway.GetAllAsync();
            return tickets.Count();
        }
        catch (TicketGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Ticket microservice for GetCountAsync");
            throw new ValidationException($"Failed to communicate with Ticket microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Ticket count");
            throw new ValidationException("Failed to get Ticket count", ex);
        }
    }

    /// <summary>
    /// Validates Ticket entity for business rules
    /// </summary>
    /// <param name="ticket">The Ticket to validate</param>
    /// <exception cref="TicketValidationException">Thrown when validation fails</exception>
    private void ValidateTicket(Ticket ticket)
    {
        if (ticket == null)
        {
            throw new TicketValidationException("Ticket cannot be null");
        }

        var errors = new Dictionary<string, string[]>();

        // Validate PassengerName
        if (string.IsNullOrWhiteSpace(ticket.PassengerName))
        {
            errors["PassengerName"] = new[] { "PassengerName is required and cannot be empty" };
        }
        else if (ticket.PassengerName.Length > 255)
        {
            errors["PassengerName"] = new[] { "PassengerName cannot exceed 255 characters" };
        }

        // Validate PassengerEmail
        if (string.IsNullOrWhiteSpace(ticket.PassengerEmail))
        {
            errors["PassengerEmail"] = new[] { "PassengerEmail is required and cannot be empty" };
        }
        else if (ticket.PassengerEmail.Length > 255)
        {
            errors["PassengerEmail"] = new[] { "PassengerEmail cannot exceed 255 characters" };
        }

        // Validate PassengerPhone (optional but must be valid format if provided)
        if (!string.IsNullOrWhiteSpace(ticket.PassengerPhone) && ticket.PassengerPhone.Length > 50)
        {
            errors["PassengerPhone"] = new[] { "PassengerPhone cannot exceed 50 characters" };
        }

        // Validate SeatNumber (optional but must be valid if provided)
        if (!string.IsNullOrWhiteSpace(ticket.SeatNumber) && ticket.SeatNumber.Length > 10)
        {
            errors["SeatNumber"] = new[] { "SeatNumber cannot exceed 10 characters" };
        }

        // Validate Price
        if (ticket.Price <= 0)
        {
            errors["Price"] = new[] { "Price must be positive" };
        }
        else if (ticket.Price > int.MaxValue)
        {
            errors["Price"] = new[] { "Price exceeds maximum allowed value" };
        }

        // Validate BookingDate (must be in the past or present)
        if (ticket.BookingDate > DateTime.UtcNow)
        {
            errors["BookingDate"] = new[] { "Booking date and time cannot be in the future" };
        }

        if (errors.Count > 0)
        {
            var errorMessage = $"Ticket validation failed with {errors.Count} error(s)";
            throw new TicketValidationException(errorMessage, errors);
        }
    }
}
