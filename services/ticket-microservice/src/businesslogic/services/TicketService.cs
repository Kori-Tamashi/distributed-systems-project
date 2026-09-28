using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;

using RepositoryTicketNotFoundException = core.exceptions.dataaccess.repositories.TicketNotFoundException;
using RepositoryTicketAlreadyExistsException = core.exceptions.dataaccess.repositories.TicketAlreadyExistsException;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Ticket business logic operations (per lab2-template v1 spec)
/// Table: ticket
/// Columns: ticket_uid, username, flight_number, price, status
/// </summary>
public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;

    /// <summary>
    /// Initializes a new instance of TicketService
    /// </summary>
    /// <param name="ticketRepository">The Ticket repository for data access</param>
    public TicketService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
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
            var ticket = await _ticketRepository.GetByIdAsync(id);
            return ticket ?? throw new TicketNotFoundException(id);
        }
        catch (RepositoryTicketNotFoundException)
        {
            throw new TicketNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to get Ticket with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<Ticket>> GetAllAsync(TicketFilter? filter = null)
    {
        try
        {
            return await _ticketRepository.GetAllAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get all Tickets", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Ticket> CreateAsync(Ticket ticket)
    {
        ValidateTicket(ticket);

        try
        {
            var createdTicket = await _ticketRepository.CreateAsync(ticket);
            return createdTicket;
        }
        catch (TicketValidationException)
        {
            throw;
        }
        catch (RepositoryTicketAlreadyExistsException)
        {
            throw new TicketBusinessRuleViolationException(
                "UniqueConstraint", 
                $"Ticket with UID {ticket.TicketUid} already exists");
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to create Ticket", ex);
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

        // Check if ticket exists before updating (outside try-catch)
        var exists = await _ticketRepository.ExistsAsync(ticket.Id);
        if (!exists)
        {
            throw new TicketNotFoundException(ticket.Id);
        }

        try
        {
            var updatedTicket = await _ticketRepository.UpdateAsync(ticket);
            return updatedTicket;
        }
        catch (RepositoryTicketNotFoundException)
        {
            throw new TicketNotFoundException(ticket.Id);
        }
        catch (TicketValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to update Ticket with ID {ticket.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new TicketValidationException($"Invalid Ticket ID: {id}. ID must be positive.");
        }

        // Check if ticket exists before deleting (outside try-catch)
        var exists = await _ticketRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new TicketNotFoundException(id);
        }

        try
        {
            return await _ticketRepository.DeleteAsync(id);
        }
        catch (RepositoryTicketNotFoundException)
        {
            throw new TicketNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to delete Ticket with ID {id}", ex);
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
            return await _ticketRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to check existence of Ticket with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(TicketFilter? filter = null)
    {
        try
        {
            return await _ticketRepository.GetCountAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get Ticket count", ex);
        }
    }

    /// <summary>
    /// Validates Ticket entity for business rules (per lab2-template v1 spec)
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

        // Validate Username
        if (string.IsNullOrWhiteSpace(ticket.Username))
        {
            errors["Username"] = new[] { "Username is required and cannot be empty" };
        }
        else if (ticket.Username.Length > 80)
        {
            errors["Username"] = new[] { "Username cannot exceed 80 characters" };
        }

        // Validate FlightNumber
        if (string.IsNullOrWhiteSpace(ticket.FlightNumber))
        {
            errors["FlightNumber"] = new[] { "FlightNumber is required and cannot be empty" };
        }
        else if (ticket.FlightNumber.Length > 20)
        {
            errors["FlightNumber"] = new[] { "FlightNumber cannot exceed 20 characters" };
        }

        // Validate Price
        if (ticket.Price <= 0)
        {
            errors["Price"] = new[] { "Price must be positive" };
        }

        if (errors.Count > 0)
        {
            var errorMessage = $"Ticket validation failed with {errors.Count} error(s)";
            throw new TicketValidationException(errorMessage, errors);
        }
    }
}
