using core.domain;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using Microsoft.Extensions.Logging;

using ServicePrivilegeNotFoundException = core.exceptions.businesslogic.services.PrivilegeNotFoundException;
using ServicePrivilegeValidationException = core.exceptions.businesslogic.services.PrivilegeValidationException;
using ServiceTicketNotFoundException = core.exceptions.businesslogic.services.TicketNotFoundException;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Ticket business logic operations
/// Provides high-level operations with validation, business rules, and error handling
/// Uses HTTP Gateway to communicate with Ticket microservice
/// </summary>
public class TicketService : ITicketService
{
    private readonly ITicketGateway _ticketGateway;
    private readonly IFlightGateway _flightGateway;
    private readonly IPrivilegeService _privilegeService;
    private readonly IPrivilegeHistoryService _privilegeHistoryService;
    private readonly ILogger<TicketService> _logger;

    /// <summary>
    /// Initializes a new instance of TicketService
    /// </summary>
    /// <param name="ticketGateway">The Ticket Gateway for HTTP communication</param>
    /// <param name="logger">Logger for SAGA tracking</param>
    public TicketService(
        ITicketGateway ticketGateway,
        IFlightGateway flightGateway,
        IPrivilegeService privilegeService,
        IPrivilegeHistoryService privilegeHistoryService,
        ILogger<TicketService> logger)
    {
        _ticketGateway = ticketGateway ?? throw new ArgumentNullException(nameof(ticketGateway));
        _flightGateway = flightGateway ?? throw new ArgumentNullException(nameof(flightGateway));
        _privilegeService = privilegeService ?? throw new ArgumentNullException(nameof(privilegeService));
        _privilegeHistoryService = privilegeHistoryService ?? throw new ArgumentNullException(nameof(privilegeHistoryService));
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

    /// <inheritdoc/>
    public async Task<(Guid ticketUid, int paidByBonuses, int paidByMoney)> BuyTicketAsync(
        string username,
        string flightNumber,
        int price,
        bool paidFromBalance)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new TicketValidationException("Username is required");
        if (string.IsNullOrWhiteSpace(flightNumber))
            throw new TicketValidationException("Flight number is required");
        if (price <= 0)
            throw new TicketValidationException("Price must be positive");

        try
        {
            // 1. Verify flight exists
            var flights = await _flightGateway.GetAllAsync();
            var flight = flights.FirstOrDefault(f => f.FlightNumber == flightNumber);
            if (flight == null)
            {
                throw new TicketValidationException($"Flight {flightNumber} not found");
            }

            // 2. Get or create user privilege
            var privileges = await _privilegeService.GetAllAsync(new core.filters.PrivilegeFilter { Username = username });
            var privilege = privileges.FirstOrDefault();
            
            int paidByBonuses = 0;
            int paidByMoney = price;
            var ticketUid = Guid.NewGuid();

            if (paidFromBalance && privilege != null && privilege.Balance > 0)
            {
                // Pay from balance: max(balance, price)
                paidByBonuses = Math.Min(privilege.Balance, price);
                paidByMoney = price - paidByBonuses;

                // Debit balance
                await _privilegeService.DebitBalanceAsync(privilege.Id, paidByBonuses, ticketUid);
            }
            else if (!paidFromBalance && privilege != null)
            {
                // Cash payment: +10% cashback
                var cashback = price / 10;
                await _privilegeService.CreditBalanceAsync(privilege.Id, cashback, ticketUid);
            }

            // 3. Create ticket
            var ticket = new Ticket
            {
                Id = 0,
                TicketUid = ticketUid,
                Username = username,
                FlightNumber = flightNumber,
                Price = price,
                Status = 0 // Paid
            };

            var createdTicket = await _ticketGateway.CreateAsync(ticket);

            _logger.LogInformation("Ticket bought: {TicketUid} by {Username}", createdTicket.TicketUid, username);
            return (createdTicket.TicketUid, paidByBonuses, paidByMoney);
        }
        catch (TicketValidationException)
        {
            throw;
        }
        catch (ServicePrivilegeNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege not found for user: {Username}", username);
            throw new TicketValidationException($"User {username} not found in bonus system");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to buy ticket for {Username}", username);
            throw new ValidationException("Failed to buy ticket", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ReturnTicketAsync(Guid ticketUid, string username)
    {
        if (ticketUid == Guid.Empty)
            throw new TicketValidationException("Ticket UID is required");
        if (string.IsNullOrWhiteSpace(username))
            throw new TicketValidationException("Username is required");

        try
        {
            // 1. Find ticket by UID
            var tickets = await _ticketGateway.GetAllAsync();
            var ticket = tickets.FirstOrDefault(t => t.TicketUid == ticketUid && t.Username == username);
            
            if (ticket == null)
            {
                throw new TicketNotFoundException(0); // Guid-based lookup, ID not available
            }

            if (ticket.Status == 1) // Already Canceled (Cancelled = 1)
            {
                throw new TicketValidationException("Ticket is already canceled");
            }

            // 2. Get privilege for cashback/refund
            var privileges = await _privilegeService.GetAllAsync(new core.filters.PrivilegeFilter { Username = username });
            var privilege = privileges.FirstOrDefault();

            if (privilege != null)
            {
                // Check privilege_history to determine what to do on return
                var historyList = await _privilegeHistoryService.GetAllAsync(
                    new core.filters.PrivilegeHistoryFilter { TicketUid = ticketUid, PrivilegeId = privilege.Id });
                
                // Find the debit operation (user paid with bonuses)
                var debitHistory = historyList
                    .Where(h => h.OperationType == core.enums.OperationType.DEBIT_THE_ACCOUNT)
                    .OrderByDescending(h => h.DateTime)
                    .FirstOrDefault();
                
                if (debitHistory != null)
                {
                    // User paid with bonuses - return the bonuses (BalanceDiff is negative for debits)
                    var bonusesToReturn = Math.Abs(debitHistory.BalanceDiff);
                    await _privilegeService.CreditBalanceAsync(privilege.Id, bonusesToReturn, ticketUid);
                    _logger.LogInformation("Returning {Bonuses} bonuses for ticket {TicketUid}", bonusesToReturn, ticketUid);
                }
                else
                {
                    // Find the credit operation (user paid with money and got cashback)
                    var creditHistory = historyList
                        .Where(h => h.OperationType == core.enums.OperationType.FILL_IN_BALANCE)
                        .OrderByDescending(h => h.DateTime)
                        .FirstOrDefault();
                    
                    if (creditHistory != null)
                    {
                        // User paid with money and got cashback - deduct the cashback
                        // But balance cannot go below 0 (per TZ: "При списании бонусный счёт не может стать меньше 0")
                        var cashbackToDeduct = Math.Min(Math.Abs(creditHistory.BalanceDiff), privilege.Balance);
                        if (cashbackToDeduct > 0)
                        {
                            await _privilegeService.DebitBalanceAsync(privilege.Id, cashbackToDeduct, ticketUid);
                            _logger.LogInformation("Deducting {Cashback} cashback for ticket {TicketUid}", cashbackToDeduct, ticketUid);
                        }
                    }
                }
            }

            // 3. Update ticket status to CANCELED
            ticket.Status = 1; // Cancelled
            await _ticketGateway.UpdateAsync(ticket);

            _logger.LogInformation("Ticket returned: {TicketUid} by {Username}", ticketUid, username);
            return true;
        }
        catch (TicketValidationException)
        {
            throw;
        }
        catch (TicketNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to return ticket: {TicketUid}", ticketUid);
            throw new ValidationException("Failed to return ticket", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Ticket> GetByIdByUserAsync(Guid ticketUid, string username)
    {
        if (ticketUid == Guid.Empty)
            throw new TicketValidationException("Ticket UID is required");
        if (string.IsNullOrWhiteSpace(username))
            throw new TicketValidationException("Username is required");

        try
        {
            var tickets = await _ticketGateway.GetAllAsync();
            var ticket = tickets.FirstOrDefault(t => t.TicketUid == ticketUid);

            if (ticket == null)
            {
                throw new ServiceTicketNotFoundException(0);
            }

            // Note: ownership check is done in controller to return 403 vs 404
            return ticket;
        }
        catch (ServiceTicketNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get ticket: {TicketUid} for user {Username}", ticketUid, username);
            throw new ValidationException("Failed to get ticket", ex);
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

        // Validate Status (PAID=1 or CANCELED=2)
        if (ticket.Status != 1 && ticket.Status != 2)
        {
            errors["Status"] = new[] { "Status must be PAID (1) or CANCELED (2)" };
        }

        if (errors.Count > 0)
        {
            var errorMessage = $"Ticket validation failed with {errors.Count} error(s)";
            throw new TicketValidationException(errorMessage, errors);
        }
    }
}
