using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Ticket;
using presentation.exceptions.http;

using ServiceTicketNotFoundException = core.exceptions.businesslogic.services.TicketNotFoundException;
using ServiceTicketValidationException = core.exceptions.businesslogic.services.TicketValidationException;
using ServiceTicketBusinessRuleViolationException = core.exceptions.businesslogic.services.TicketBusinessRuleViolationException;

using HttpTicketNotFoundException = presentation.exceptions.http.TicketNotFoundException;
using HttpTicketValidationException = presentation.exceptions.http.TicketValidationException;
using HttpTicketBusinessRuleViolationException = presentation.exceptions.http.TicketBusinessRuleViolationException;

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

    /// <summary>
    /// Initializes a new instance of the TicketHttpController
    /// </summary>
    /// <param name="ticketService">The Ticket business logic service</param>
    /// <param name="logger">The logger for the controller</param>
    public TicketHttpController(
        ITicketService ticketService,
        ILogger<TicketHttpController> logger)
    {
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
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
        [FromQuery] int? pageSize,
        [FromQuery] string? username)
    {
        try
        {
            _logger.LogDebug("Getting all tickets");
            
            var tickets = await _ticketService.GetAllAsync(new core.filters.TicketFilter { Username = username });
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TicketDTO>> CreateTicket(
        [FromBody] CreateTicketDTO createDto)
    {
        try
        {
            _logger.LogDebug("Creating new ticket");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for ticket creation");
                var validationErrors = new Dictionary<string, string[]>();
                foreach (var keyValuePair in ModelState)
                {
                    var key = keyValuePair.Key ?? "Unknown";
                    foreach (var error in keyValuePair.Value.Errors)
                    {
                        if (!validationErrors.ContainsKey(key))
                        {
                            validationErrors[key] = Array.Empty<string>();
                        }
                        validationErrors[key] = validationErrors[key].Concat(new[] { error.ErrorMessage }).ToArray();
                    }
                }
                
                return BadRequest(new HttpTicketValidationException(validationErrors));
            }
            
            // Convert DTO to domain entity
            var ticket = TicketHttpConverter.ToCreateDomain(createDto);
            
            // Create the ticket
            var createdTicket = await _ticketService.CreateAsync(ticket);
            
            // Build location URI
            var location = Url.Action(nameof(GetTicketById), new { ticketId = createdTicket.Id });
            var dto = TicketHttpConverter.ToDTO(createdTicket);
            
            _logger.LogInformation("Ticket created successfully with ID: {TicketId}", createdTicket.Id);
            return CreatedAtAction(nameof(GetTicketById), new { ticketId = createdTicket.Id }, dto);
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
            _logger.LogWarning(ex, "Ticket business rule violated");
            return Conflict(new HttpTicketBusinessRuleViolationException(ex.RuleName, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ticket: {Error}", ex.Message);
            throw new TicketInternalServerException(ex);
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
    /// <response code="409">Ticket already exists</response>
    /// <response code="500">Server error</response>
    [HttpPut("{ticketId}")]
    [ProducesResponseType(typeof(TicketDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpTicketValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpTicketNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpTicketBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TicketDTO>> UpdateTicket(
        int ticketId,
        [FromBody] UpdateTicketDTO updateDto)
    {
        try
        {
            _logger.LogDebug("Updating ticket with ID: {TicketId}", ticketId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for ticket update");
                var validationErrors = new Dictionary<string, string[]>();
                foreach (var keyValuePair in ModelState)
                {
                    var key = keyValuePair.Key ?? "Unknown";
                    foreach (var error in keyValuePair.Value.Errors)
                    {
                        if (!validationErrors.ContainsKey(key))
                        {
                            validationErrors[key] = Array.Empty<string>();
                        }
                        validationErrors[key] = validationErrors[key].Concat(new[] { error.ErrorMessage }).ToArray();
                    }
                }
                
                return BadRequest(new HttpTicketValidationException(validationErrors));
            }
            
            // Convert DTO to domain entity (partial update)
            var existingTicket = await _ticketService.GetByIdAsync(ticketId);
            var ticket = TicketHttpConverter.ToUpdateDomain(updateDto, existingTicket);
            
            // Update the ticket
            var updatedTicket = await _ticketService.UpdateAsync(ticket);
            var dto = TicketHttpConverter.ToDTO(updatedTicket);
            
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
            _logger.LogWarning(ex, "Ticket validation failed during update");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpTicketValidationException(errorData));
        }
        catch (ServiceTicketBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Ticket business rule violated during update");
            return Conflict(new HttpTicketBusinessRuleViolationException(ex.RuleName, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket: {TicketId}", ticketId);
            throw new TicketInternalServerException(ex);
        }
    }

    /// <summary>
    /// Deletes a Ticket by their unique identifier
    /// </summary>
    /// <param name="ticketId">The unique identifier of the Ticket to delete</param>
    /// <returns>HTTP 200 OK with confirmation</returns>
    /// <response code="200">Ticket deleted successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Ticket not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{ticketId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpTicketNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpTicketValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteTicket(int ticketId)
    {
        try
        {
            _logger.LogDebug("Deleting ticket with ID: {TicketId}", ticketId);
            
            await _ticketService.DeleteAsync(ticketId);
            
            _logger.LogInformation("Ticket deleted successfully: {TicketId}", ticketId);
            return Ok(new { message = $"Ticket with ID {ticketId} was deleted successfully" });
        }
        catch (ServiceTicketNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ticket not found for deletion: {TicketId}", ticketId);
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
            _logger.LogError(ex, "Error deleting ticket: {TicketId} - {Error}", ticketId, ex.Message);
            throw new TicketInternalServerException(ex);
        }
    }
}
