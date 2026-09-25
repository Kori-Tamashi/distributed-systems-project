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

using HttpTicketNotFoundException = presentation.exceptions.http.Ticket.TicketNotFoundException;
using HttpTicketValidationException = presentation.exceptions.http.Ticket.TicketValidationException;
using HttpTicketBusinessRuleViolationException = presentation.exceptions.http.Ticket.TicketBusinessRuleViolationException;
using HttpTicketAccessForbiddenException = presentation.exceptions.http.Ticket.TicketAccessForbiddenException;

using System.Linq;

namespace presentation.controllers.http;

/// <summary>
/// HTTP Controller for Ticket operations per lab2-template v1 spec
/// Endpoints: GET /tickets, GET /tickets/{ticketUid}, POST /tickets (BuyTicket), DELETE /tickets/{ticketUid} (ReturnTicket)
/// </summary>
[ApiController]
[Route("api/v1/tickets")]
[Produces("application/json")]
[Consumes("application/json")]
public class TicketHttpController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketHttpController> _logger;

    public TicketHttpController(
        ITicketService ticketService,
        ILogger<TicketHttpController> logger)
    {
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all tickets for the current user (X-User-Name header required)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TicketDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<TicketDTO>>> GetMyTickets()
    {
        var username = Request.Headers["X-User-Name"].ToString();
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new ErrorResponse("X-User-Name header is required"));
        }

        try
        {
            _logger.LogDebug("Getting tickets for user: {Username}", username);
            
            var tickets = await _ticketService.GetAllAsync(new core.filters.TicketFilter { Username = username });
            var dtos = TicketHttpConverter.ToDTO(tickets);
            
            _logger.LogInformation("Retrieved {Count} tickets for user {Username}", dtos.Count, username);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tickets for user {Username}", username);
            throw new TicketInternalServerException(ex);
        }
    }

    /// <summary>
    /// Gets a ticket by UID (checks ownership via X-User-Name header)
    /// </summary>
    [HttpGet("{ticketUid:guid}")]
    [ProducesResponseType(typeof(TicketDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpTicketNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpTicketAccessForbiddenException), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TicketDTO>> GetTicketById(Guid ticketUid)
    {
        var username = Request.Headers["X-User-Name"].ToString();
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new ErrorResponse("X-User-Name header is required"));
        }

        try
        {
            _logger.LogDebug("Getting ticket: {TicketUid} for user {Username}", ticketUid, username);
            
            var ticket = await _ticketService.GetByIdByUserAsync(ticketUid, username);
            
            // Check ownership - return 403 if ticket belongs to another user
            if (ticket.Username != username)
            {
                return StatusCode(403, new ErrorResponse($"Ticket {ticketUid} access forbidden"));
            }
            
            var dto = TicketHttpConverter.ToDTO(ticket);
            
            _logger.LogInformation("Ticket retrieved: {TicketUid} by {Username}", ticketUid, username);
            return Ok(dto);
        }
        catch (ServiceTicketNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ticket not found: {TicketUid}", ticketUid);
            return NotFound(new ErrorResponse($"Ticket {ticketUid} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket: {TicketUid}", ticketUid);
            throw new TicketInternalServerException(ex);
        }
    }

    /// <summary>
    /// Buys a ticket (per lab2-template v1 spec)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BuyTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpTicketValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BuyTicketResponse>> BuyTicket([FromBody] BuyTicketRequest request)
    {
        try
        {
            var username = Request.Headers["X-User-Name"].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new ValidationErrorResponse("X-User-Name header is required"));
            }

            _logger.LogDebug("Buying ticket for user: {Username}", username);

            var (ticketUid, paidByBonuses, paidByMoney) = await _ticketService.BuyTicketAsync(
                username,
                request.FlightNumber,
                request.Price,
                request.PaidFromBalance);

            var response = new BuyTicketResponse
            {
                TicketUid = ticketUid,
                PaidByBonuses = paidByBonuses,
                PaidByMoney = paidByMoney,
                Username = username,
                FlightNumber = request.FlightNumber,
                Price = request.Price
            };

            _logger.LogInformation("Ticket bought: {TicketUid} by {Username}", ticketUid, username);
            return Created($"/api/v1/tickets/{ticketUid}", response);
        }
        catch (ServiceTicketValidationException ex)
        {
            _logger.LogWarning(ex, "Ticket validation failed");
            return BadRequest(new ValidationErrorResponse(ex.Message, ex.Errors?.SelectMany(kvp => kvp.Value).ToList() ?? new List<string>()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buying ticket");
            throw new TicketInternalServerException(ex);
        }
    }

    /// <summary>
    /// Returns (cancels) a ticket (per lab2-template v1 spec)
    /// </summary>
    [HttpDelete("{ticketUid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpTicketValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpTicketNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ReturnTicket(Guid ticketUid)
    {
        try
        {
            var username = Request.Headers["X-User-Name"].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new ValidationErrorResponse("X-User-Name header is required"));
            }

            _logger.LogDebug("Returning ticket: {TicketUid} by {Username}", ticketUid, username);

            var result = await _ticketService.ReturnTicketAsync(ticketUid, username);

            _logger.LogInformation("Ticket returned: {TicketUid} by {Username}", ticketUid, username);
            return NoContent();
        }
        catch (ServiceTicketValidationException ex)
        {
            _logger.LogWarning(ex, "Ticket validation failed for return: {TicketUid}", ticketUid);
            return BadRequest(new ValidationErrorResponse(ex.Message));
        }
        catch (ServiceTicketNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ticket not found for return: {TicketUid}", ticketUid);
            return NotFound(new ErrorResponse($"Ticket {ticketUid} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning ticket: {TicketUid}", ticketUid);
            throw new TicketInternalServerException(ex);
        }
    }
}
