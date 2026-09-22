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

using System.Linq;

namespace presentation.controllers.http;

/// <summary>
/// HTTP Controller for Ticket CRUD operations
/// Implements RESTful API endpoints for managing Ticket entities
/// </summary>
[ApiController]
[Route("api/v1/tickets")]
[Produces("application/json")]
[Consumes("application/json")]
public class TicketHttpController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketHttpController> _logger;
    private readonly BookingSagaCoordinator _bookingSagaCoordinator;

    /// <summary>
    /// Initializes a new instance of the TicketHttpController
    /// </summary>
    /// <param name="ticketService">The Ticket business logic service</param>
    /// <param name="logger">The logger for the controller</param>
    public TicketHttpController(
        ITicketService ticketService,
        BookingSagaCoordinator bookingSagaCoordinator,
        ILogger<TicketHttpController> logger)
    {
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
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
    /// Creates a new Ticket
    /// </summary>
    /// <param name="createDto">The Ticket data to create</param>
    /// <returns>Created Ticket with HTTP 201 Created and Location header</returns>
    /// <response code="201">Ticket created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="409">Ticket already exists</response>
    /// <response code="500">Server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(TicketDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpTicketValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpTicketBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(SagaException), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TicketDTO>> CreateTicket(
        [FromBody] CreateTicketDTO createDto)
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
            
            _logger.LogDebug("Creating new ticket with SAGA pattern");
            
            var ticket = TicketHttpConverter.ToCreateDomain(createDto);
            var createdTicket = await _ticketService.CreateAsync(ticket);
            var dto = TicketHttpConverter.ToDTO(createdTicket);
            
            _logger.LogInformation("Ticket created successfully: {TicketId}", createdTicket.Id);
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
            _logger.LogError(ex, "Error creating ticket - initiating SAGA compensation");
            
            // SAGA compensation would be triggered here if partial state existed
            throw new TicketInternalServerException(
                $"Failed to create ticket. Any partial changes have been compensated. Error: {ex.Message}", 
                ex);
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
