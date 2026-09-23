using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Ticket;
using presentation.exceptions.http;
using presentation.exceptions.http.Ticket;

using ServiceTicketNotFoundException = core.exceptions.businesslogic.services.TicketNotFoundException;
using ServiceTicketValidationException = core.exceptions.businesslogic.services.TicketValidationException;
using ServiceTicketBusinessRuleViolationException = core.exceptions.businesslogic.services.TicketBusinessRuleViolationException;
using ServicePrivilegeNotFoundException = core.exceptions.businesslogic.services.PrivilegeNotFoundException;

using HttpTicketNotFoundException = presentation.exceptions.http.Ticket.TicketNotFoundException;
using HttpTicketValidationException = presentation.exceptions.http.Ticket.TicketValidationException;
using HttpTicketBusinessRuleViolationException = presentation.exceptions.http.Ticket.TicketBusinessRuleViolationException;

using System.Linq;

namespace presentation.controllers.http;

/// <summary>
/// HTTP Controller for Ticket operations with SAGA pattern
/// Implements distributed transactions across Flight, Ticket, and Bonus services
/// </summary>
[ApiController]
[Route("api/v1/tickets")]
[Produces("application/json")]
[Consumes("application/json")]
public class TicketHttpController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IFlightService _flightService;
    private readonly IPrivilegeService _privilegeService;
    private readonly ILogger<TicketHttpController> _logger;
    private readonly BookingSagaCoordinator _bookingSagaCoordinator;

    /// <summary>
    /// Initializes a new instance of the TicketHttpController
    /// </summary>
    public TicketHttpController(
        ITicketService ticketService,
        IFlightService flightService,
        IPrivilegeService privilegeService,
        BookingSagaCoordinator bookingSagaCoordinator,
        ILogger<TicketHttpController> logger)
    {
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        _flightService = flightService ?? throw new ArgumentNullException(nameof(flightService));
        _privilegeService = privilegeService ?? throw new ArgumentNullException(nameof(privilegeService));
        _bookingSagaCoordinator = bookingSagaCoordinator ?? throw new ArgumentNullException(nameof(bookingSagaCoordinator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a Ticket by their unique identifier
    /// </summary>
    /// <param name="ticketId">The unique identifier of the Ticket</param>
    /// <returns>Ticket data with HTTP 200 OK</returns>
    /// <response code="200">Returns the Ticket</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Ticket not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("{ticketId}")]
    [ProducesResponseType(typeof(TicketDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpTicketNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpTicketValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TicketDTO>> GetTicketById(int ticketId)
    {
        try
        {
            _logger.LogDebug("Getting ticket by ID: {TicketId}", ticketId);
            
            var ticket = await _ticketService.GetByIdAsync(ticketId);
            var dto = TicketHttpConverter.ToDTO(ticket);
            
            _logger.LogInformation("Ticket retrieved successfully: {TicketId}", ticketId);
            return Ok(dto);
        }
        catch (ServiceTicketNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ticket not found: {TicketId}", ticketId);
            return NotFound(new HttpTicketNotFoundException(ticketId));
        }
        catch (ServiceTicketValidationException ex)
        {
            _logger.LogWarning(ex, "Ticket validation failed for ID: {TicketId}", ticketId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpTicketValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket by ID: {TicketId}", ticketId);
            throw new TicketInternalServerException(ex);
        }
    }

    /// <summary>
    /// Gets all Tickets
    /// </summary>
    /// <returns>List of all Tickets with HTTP 200 OK</returns>
    /// <response code="200">Returns the list of Tickets</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TicketDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<TicketDTO>>> GetAllTickets(
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        try
        {
            _logger.LogDebug("Getting all tickets");
            
            var tickets = await _ticketService.GetAllAsync();
            var totalCount = tickets.Count;
            
            // Apply pagination if requested
            if (page.HasValue && pageSize.HasValue && pageSize.Value > 0)
            {
                tickets = tickets.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            }
            
            var dtos = TicketHttpConverter.ToDTO(tickets);
            
            _logger.LogInformation("Retrieved {Count} tickets", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tickets");
            throw new TicketInternalServerException(ex);
        }
    }

    /// <summary>
    /// Buys a ticket using SAGA pattern across Flight, Ticket, and Bonus services
    /// </summary>
    /// <param name="buyDto">Ticket purchase data with flight number and payment method</param>
    /// <returns>Created Ticket with HTTP 201 Created</returns>
    /// <response code="201">Ticket purchased successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Flight not found or user privilege not found</response>
    /// <response code="502">Bonus service unavailable - ticket created but bonus operation failed</response>
    /// <response code="500">SAGA failed with compensation</response>
    [HttpPost]
    [ProducesResponseType(typeof(TicketDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpTicketValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpTicketBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(SagaException), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TicketDTO>> BuyTicket(
        [FromBody] dataaccess.dto.http.Ticket.BuyTicketDTO buyDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorData = ModelState
                    .Where(x => x.Value != null && x.Value!.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new HttpTicketValidationException(errorData));
            }
            
            _logger.LogInformation("Starting SAGA for ticket purchase: FlightNumber={FlightNumber}, Passenger={PassengerName}", 
                buyDto.FlightNumber, buyDto.PassengerName);
            
            // Step 1: Get username from header
            var username = Request.Headers["X-User-Name"].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("X-User-Name header is missing");
                return BadRequest(new HttpTicketValidationException(
                    new Dictionary<string, string[]> { { "X-User-Name", new[] { "Header X-User-Name is required" } } }));
            }
            
            // Step 2: Check flight exists (CRITICAL)
            _logger.LogDebug("SAGA Step 1: Checking flight {FlightNumber}", buyDto.FlightNumber);
            var flights = await _flightService.GetAllAsync(new core.filters.FlightFilter 
            { 
                FlightNumber = buyDto.FlightNumber 
            });
            
            if (!flights.Any())
            {
                _logger.LogWarning("Flight not found: {FlightNumber}", buyDto.FlightNumber);
                return NotFound(new { Message = $"Flight {buyDto.FlightNumber} not found" });
            }
            
            var flight = flights.First();
            _logger.LogInformation("Flight found: FlightId={FlightId}", flight.Id);
            
            // Step 3: Get or create user privilege (for bonus operations)
            _logger.LogDebug("SAGA Step 1.5: Getting privilege for user {Username}", username);
            var privileges = await _privilegeService.GetAllAsync(new core.filters.PrivilegeFilter 
            { 
                Username = username 
            });
            
            core.domain.Privilege? privilege = null;
            if (privileges.Any())
            {
                privilege = privileges.First();
            }
            else
            {
                _logger.LogInformation("Creating new privilege for user {Username}", username);
                privilege = await _privilegeService.CreateAsync(new core.domain.Privilege
                {
                    Username = username,
                    Status = core.enums.PrivilegeStatus.BRONZE,
                    Balance = 0
                });
            }
            
            // Step 4: Create ticket (CRITICAL)
            _logger.LogDebug("SAGA Step 2: Creating ticket");
            var ticket = new core.domain.Ticket
            {
                TicketUid = Guid.NewGuid(),
                FlightId = flight.Id,
                PassengerName = buyDto.PassengerName,
                PassengerEmail = buyDto.PassengerEmail,
                PassengerPhone = buyDto.PassengerPhone,
                SeatNumber = buyDto.SeatNumber,
                Class = (core.enums.TicketClass)buyDto.Class,
                Price = buyDto.Price,
                BookingDate = buyDto.BookingDate,
                Status = 0 // ACTIVE
            };
            
            var createdTicket = await _ticketService.CreateAsync(ticket);
            _logger.LogInformation("Ticket created: TicketId={TicketId}, TicketUid={TicketUid}", 
                createdTicket.Id, createdTicket.TicketUid);
            
            // Step 5: Process bonus payment (NON-CRITICAL with rollback)
            try
            {
                _logger.LogDebug("SAGA Step 3: Processing bonus payment. PaidFromBalance={PaidFromBalance}", 
                    buyDto.PaidFromBalance);
                
                if (buyDto.PaidFromBalance)
                {
                    // Debit points from user's balance
                    if (buyDto.Price > privilege.Balance)
                    {
                        await _ticketService.DeleteAsync(createdTicket.Id);
                        return BadRequest(new { Message = "Insufficient bonus balance" });
                    }
                    
                    await _privilegeService.DebitBalanceAsync(privilege.Id, buyDto.Price, createdTicket.TicketUid);
                    _logger.LogInformation("Debit successful: Amount={Amount}", buyDto.Price);
                }
                else
                {
                    // Credit 10% cashback
                    var cashback = buyDto.Price / 10;
                    await _privilegeService.CreditBalanceAsync(privilege.Id, cashback, createdTicket.TicketUid);
                    _logger.LogInformation("Cashback credited: Amount={Cashback}", cashback);
                }
            }
            catch (PrivilegeNotFoundException ex)
            {
                _logger.LogError(ex, "Privilege not found - rolling back ticket");
                await _ticketService.DeleteAsync(createdTicket.Id);
                return NotFound(new { Message = "User privilege not found" });
            }
            catch (Exception ex) when (ex is not ServicePrivilegeNotFoundException)
            {
                _logger.LogError(ex, "Bonus service unavailable - rolling back ticket");
                await _ticketService.DeleteAsync(createdTicket.Id);
                return StatusCode(502, new 
                { 
                    Message = "Ticket created but bonus service unavailable. Ticket has been canceled.",
                    TicketUid = createdTicket.TicketUid,
                    CompensationStatus = "COMPLETED"
                });
            }
            
            _logger.LogInformation("SAGA completed successfully: TicketUid={TicketUid}", createdTicket.TicketUid);
            var dto = TicketHttpConverter.ToDTO(createdTicket);
            
            return CreatedAtAction(
                nameof(GetTicketById),
                new { ticketId = createdTicket.Id },
                dto);
        }
        catch (ServiceTicketValidationException ex)
        {
            _logger.LogWarning(ex, "Ticket validation failed");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpTicketValidationException(errorData));
        }
        catch (ServiceTicketBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Ticket business rule violation");
            return Conflict(new HttpTicketBusinessRuleViolationException(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SAGA failed - compensating transactions executed");
            throw new SagaException(
                $"Failed to purchase ticket. Compensating transactions have been executed. Error: {ex.Message}",
                failedStep: 0,
                compensatedOperations: new List<string> { "Ticket creation", "Bonus operation" },
                innerException: ex);
        }
    }

    /// <summary>
    /// Updates an existing Ticket
    /// </summary>
    /// <param name="ticketId">The unique identifier of the Ticket to update</param>
    /// <param name="updateDto">The updated Ticket data</param>
    /// <returns>Updated Ticket with HTTP 200 OK</returns>
    /// <response code="200">Ticket updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Ticket not found</response>
    /// <response code="500">Server error</response>
    [HttpPut("{ticketId}")]
    [ProducesResponseType(typeof(TicketDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpTicketNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpTicketValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TicketDTO>> UpdateTicket(
        int ticketId,
        [FromBody] UpdateTicketDTO updateDto)
    {
        try
        {
            _logger.LogDebug("Updating ticket: {TicketId}", ticketId);
            
            var existingTicket = await _ticketService.GetByIdAsync(ticketId);
            
            // Ensure IDs match
            if (existingTicket != null && existingTicket.Id != ticketId)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} != entity ID {EntityId}", ticketId, existingTicket.Id);
                var errorData = new Dictionary<string, string[]> { { "id", new[] { "Route ID must match entity ID" } } };
                return BadRequest(new HttpTicketValidationException(errorData));
            }
            
            var updatedTicket = TicketHttpConverter.ToUpdateDomain(updateDto, existingTicket);
            var result = await _ticketService.UpdateAsync(updatedTicket);
            var dto = TicketHttpConverter.ToDTO(result);
            
            _logger.LogInformation("Ticket updated successfully: {TicketId}", ticketId);
            return Ok(dto);
        }
        catch (ServiceTicketNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ticket not found for update: {TicketId}", ticketId);
            return NotFound(new HttpTicketNotFoundException(ticketId));
        }
        catch (ServiceTicketValidationException ex)
        {
            _logger.LogWarning(ex, "Ticket validation failed for ID: {TicketId}", ticketId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpTicketValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket: {TicketId}", ticketId);
            throw new TicketInternalServerException(ex);
        }
    }

    /// <summary>
    /// Deletes a Ticket
    /// </summary>
    /// <param name="ticketId">The unique identifier of the Ticket to delete</param>
    /// <returns>HTTP 204 No Content on success</returns>
    /// <response code="204">Ticket deleted successfully</response>
    /// <response code="404">Ticket not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{ticketId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpTicketNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteTicket(int ticketId)
    {
        try
        {
            _logger.LogDebug("Deleting ticket: {TicketId}", ticketId);
            
            await _ticketService.DeleteAsync(ticketId);
            
            _logger.LogInformation("Ticket deleted successfully: {TicketId}", ticketId);
            return NoContent();
        }
        catch (ServiceTicketNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ticket not found for deletion: {TicketId}", ticketId);
            return NotFound(new HttpTicketNotFoundException(ticketId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ticket: {TicketId}", ticketId);
            throw new TicketInternalServerException(ex);
        }
    }
}
