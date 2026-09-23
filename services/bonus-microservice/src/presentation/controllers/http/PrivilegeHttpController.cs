using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Privilege;
using presentation.exceptions.http;

using ServicePrivilegeNotFoundException = core.exceptions.businesslogic.services.PrivilegeNotFoundException;
using ServicePrivilegeValidationException = core.exceptions.businesslogic.services.PrivilegeValidationException;
using ServicePrivilegeBusinessRuleViolationException = core.exceptions.businesslogic.services.PrivilegeBusinessRuleViolationException;

using HttpPrivilegeNotFoundException = presentation.exceptions.http.PrivilegeNotFoundException;
using HttpPrivilegeValidationException = presentation.exceptions.http.PrivilegeValidationException;
using HttpPrivilegeBusinessRuleViolationException = presentation.exceptions.http.PrivilegeBusinessRuleViolationException;

using System.Linq;

namespace presentation.controllers.http;

/// <summary>
/// HTTP Controller for Privilege CRUD operations
/// Implements RESTful API endpoints for managing Privilege entities
/// </summary>
[ApiController]
[Route("api/v1/privileges")]
[Produces("application/json")]
[Consumes("application/json")]
public class PrivilegeHttpController : ControllerBase
{
    private readonly IPrivilegeService _privilegeService;
    private readonly ILogger<PrivilegeHttpController> _logger;

    /// <summary>
    /// Initializes a new instance of the PrivilegeHttpController
    /// </summary>
    /// <param name="privilegeService">The Privilege business logic service</param>
    /// <param name="logger">The logger for the controller</param>
    public PrivilegeHttpController(
        IPrivilegeService privilegeService,
        ILogger<PrivilegeHttpController> logger)
    {
        _privilegeService = privilegeService ?? throw new ArgumentNullException(nameof(privilegeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a Privilege by their unique identifier
    /// </summary>
    /// <param name="privilegeId">The unique identifier of the Privilege</param>
    /// <returns>Privilege data with HTTP 200 OK</returns>
    /// <response code="200">Returns the Privilege</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Privilege not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("{privilegeId}")]
    [ProducesResponseType(typeof(PrivilegeDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpPrivilegeNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpPrivilegeValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrivilegeDTO>> GetPrivilegeById(int privilegeId)
    {
        try
        {
            _logger.LogDebug("Getting privilege by ID: {PrivilegeId}", privilegeId);
            
            var privilege = await _privilegeService.GetByIdAsync(privilegeId);
            var dto = PrivilegeHttpConverter.ToDTO(privilege);
            
            _logger.LogInformation("Privilege retrieved successfully: {PrivilegeId}", privilegeId);
            return Ok(dto);
        }
        catch (ServicePrivilegeNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege not found: {PrivilegeId}", privilegeId);
            return NotFound(new HttpPrivilegeNotFoundException(privilegeId));
        }
        catch (ServicePrivilegeValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege validation failed for ID: {PrivilegeId}", privilegeId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting privilege by ID: {PrivilegeId}", privilegeId);
            throw new PrivilegeInternalServerException(ex);
        }
    }

    /// <summary>
    /// Gets all Privileges
    /// </summary>
    /// <returns>List of all Privileges with HTTP 200 OK</returns>
    /// <response code="200">Returns the list of Privileges</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<PrivilegeDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<PrivilegeDTO>>> GetAllPrivileges(
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        try
        {
            _logger.LogDebug("Getting all privileges");
            
            var privileges = await _privilegeService.GetAllAsync();
            var totalCount = privileges.Count;
            
            // Apply pagination if requested
            if (page.HasValue && pageSize.HasValue && pageSize.Value > 0)
            {
                privileges = privileges.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            }
            
            var dtos = PrivilegeHttpConverter.ToDTO(privileges);
            
            _logger.LogInformation("Retrieved {Count} privileges", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all privileges");
            throw new PrivilegeInternalServerException(ex);
        }
    }

    /// <summary>
    /// Creates a new Privilege
    /// </summary>
    /// <param name="createDto">The Privilege data to create</param>
    /// <returns>Created Privilege with HTTP 201 Created and Location header</returns>
    /// <response code="201">Privilege created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="409">Privilege already exists</response>
    /// <response code="500">Server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(PrivilegeDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpPrivilegeValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpPrivilegeBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrivilegeDTO>> CreatePrivilege(
        [FromBody] CreatePrivilegeDTO createDto)
    {
        try
        {
            // Validate ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Privilege creation failed due to model state validation errors");
                var errorData = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new HttpPrivilegeValidationException(errorData));
            }
            
            _logger.LogDebug("Creating new privilege");
            
            var privilege = PrivilegeHttpConverter.ToCreateDomain(createDto);
            var createdPrivilege = await _privilegeService.CreateAsync(privilege);
            
            
            var dto = PrivilegeHttpConverter.ToDTO(createdPrivilege);
            
            
            _logger.LogInformation("Privilege created successfully: {PrivilegeId}", createdPrivilege.Id);
            return CreatedAtAction(
                nameof(GetPrivilegeById),
                new { privilegeId = createdPrivilege.Id },
                dto);
        }
        catch (ServicePrivilegeValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege validation failed");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeValidationException(errorData));
        }
        catch (ServicePrivilegeBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Privilege business rule violation");
            return Conflict(new HttpPrivilegeBusinessRuleViolationException(ex.RuleName, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating privilege");
            throw new PrivilegeInternalServerException(ex);
        }
    }

    /// <summary>
    /// Updates an existing Privilege
    /// </summary>
    /// <param name="privilegeId">The unique identifier of the Privilege</param>
    /// <param name="updateDto">The Privilege data to update</param>
    /// <returns>Updated Privilege with HTTP 200 OK</returns>
    /// <response code="200">Privilege updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Privilege not found</response>
    /// <response code="500">Server error</response>
    [HttpPut("{privilegeId}")]
    [ProducesResponseType(typeof(PrivilegeDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpPrivilegeNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpPrivilegeValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrivilegeDTO>> UpdatePrivilege(
        int privilegeId,
        [FromBody] UpdatePrivilegeDTO updateDto)
    {
        try
        {
            // Validate ID match
            if (privilegeId != updateDto.Id)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} != DTO ID {DtoId}", privilegeId, updateDto.Id);
                var errorData = new Dictionary<string, string[]> { { "Id", new[] { "Route ID and DTO ID must match" } } };
                return BadRequest(new HttpPrivilegeValidationException(errorData));
            }
            
            // Validate ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Privilege update failed due to model state validation errors");
                var errorData = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new HttpPrivilegeValidationException(errorData));
            }
            
            _logger.LogDebug("Updating privilege by ID: {PrivilegeId}", privilegeId);
            
            var existingPrivilege = await _privilegeService.GetByIdAsync(privilegeId);
            var updatedPrivilege = await _privilegeService.UpdateAsync(PrivilegeHttpConverter.ToUpdateDomain(updateDto, existingPrivilege));
            var dto = PrivilegeHttpConverter.ToDTO(updatedPrivilege);
            
            _logger.LogInformation("Privilege updated successfully: {PrivilegeId}", privilegeId);
            return Ok(dto);
        }
        catch (ServicePrivilegeNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege not found for update: {PrivilegeId}", privilegeId);
            return NotFound(new HttpPrivilegeNotFoundException(privilegeId));
        }
        catch (ServicePrivilegeValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege validation failed for ID: {PrivilegeId}", privilegeId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating privilege by ID: {PrivilegeId}", privilegeId);
            throw new PrivilegeInternalServerException(ex);
        }
    }

    /// <summary>
    /// Deletes a Privilege by their unique identifier
    /// </summary>
    /// <param name="privilegeId">The unique identifier of the Privilege</param>
    /// <returns>HTTP 204 No Content on success</returns>
    /// <response code="204">Privilege deleted successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Privilege not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{privilegeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpPrivilegeNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpPrivilegeValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeletePrivilege(int privilegeId)
    {
        try
        {
            _logger.LogDebug("Deleting privilege by ID: {PrivilegeId}", privilegeId);
            
            await _privilegeService.DeleteAsync(privilegeId);
            
            _logger.LogInformation("Privilege deleted successfully: {PrivilegeId}", privilegeId);
            return NoContent();
        }
        catch (ServicePrivilegeNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege not found for deletion: {PrivilegeId}", privilegeId);
            return NotFound(new HttpPrivilegeNotFoundException(privilegeId));
        }
        catch (ServicePrivilegeValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege validation failed for ID: {PrivilegeId}", privilegeId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting privilege by ID: {PrivilegeId}", privilegeId);
            throw new PrivilegeInternalServerException(ex);
        }
    }

    /// <summary>
    /// Credits balance to a privilege
    /// </summary>
    /// <param name="privilegeId">The unique identifier of the Privilege</param>
    /// <param name="amount">Amount to credit (positive)</param>
    /// <param name="ticketUid">Ticket UID associated with the operation</param>
    /// <returns>Updated Privilege with HTTP 200 OK</returns>
    /// <response code="200">Balance credited successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Privilege not found</response>
    /// <response code="500">Server error</response>
    [HttpPost("{privilegeId}/credit")]
    [ProducesResponseType(typeof(PrivilegeDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpPrivilegeNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpPrivilegeValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrivilegeDTO>> CreditBalance(
        int privilegeId,
        [FromQuery] int amount,
        [FromQuery] Guid ticketUid)
    {
        try
        {
            _logger.LogDebug("Crediting balance to privilege: {PrivilegeId}, Amount: {Amount}", privilegeId, amount);
            
            var updatedPrivilege = await _privilegeService.CreditBalanceAsync(privilegeId, amount, ticketUid);
            var dto = PrivilegeHttpConverter.ToDTO(updatedPrivilege);
            
            _logger.LogInformation("Balance credited successfully: {PrivilegeId}, New balance: {Balance}", privilegeId, updatedPrivilege.Balance);
            return Ok(dto);
        }
        catch (ServicePrivilegeNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege not found for credit: {PrivilegeId}", privilegeId);
            return NotFound(new HttpPrivilegeNotFoundException(privilegeId));
        }
        catch (ServicePrivilegeValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege validation failed for credit: {PrivilegeId}", privilegeId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crediting balance to privilege: {PrivilegeId}", privilegeId);
            throw new PrivilegeInternalServerException(ex);
        }
    }

    /// <summary>
    /// Debits balance from a privilege
    /// </summary>
    /// <param name="privilegeId">The unique identifier of the Privilege</param>
    /// <param name="amount">Amount to debit (positive)</param>
    /// <param name="ticketUid">Ticket UID associated with the operation</param>
    /// <returns>Updated Privilege with HTTP 200 OK</returns>
    /// <response code="200">Balance debited successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Privilege not found</response>
    /// <response code="409">Insufficient balance</response>
    /// <response code="500">Server error</response>
    [HttpPost("{privilegeId}/debit")]
    [ProducesResponseType(typeof(PrivilegeDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpPrivilegeNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpPrivilegeValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpPrivilegeBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrivilegeDTO>> DebitBalance(
        int privilegeId,
        [FromQuery] int amount,
        [FromQuery] Guid ticketUid)
    {
        try
        {
            _logger.LogDebug("Debiting balance from privilege: {PrivilegeId}, Amount: {Amount}", privilegeId, amount);
            
            var updatedPrivilege = await _privilegeService.DebitBalanceAsync(privilegeId, amount, ticketUid);
            var dto = PrivilegeHttpConverter.ToDTO(updatedPrivilege);
            
            _logger.LogInformation("Balance debited successfully: {PrivilegeId}, New balance: {Balance}", privilegeId, updatedPrivilege.Balance);
            return Ok(dto);
        }
        catch (ServicePrivilegeNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege not found for debit: {PrivilegeId}", privilegeId);
            return NotFound(new HttpPrivilegeNotFoundException(privilegeId));
        }
        catch (ServicePrivilegeValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege validation failed for debit: {PrivilegeId}", privilegeId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeValidationException(errorData));
        }
        catch (ServicePrivilegeBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Insufficient balance for debit: {PrivilegeId}", privilegeId);
            return Conflict(new HttpPrivilegeBusinessRuleViolationException(ex.RuleName, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error debiting balance from privilege: {PrivilegeId}", privilegeId);
            throw new PrivilegeInternalServerException(ex);
        }
    }

    /// <summary>
    /// Gets the maximum amount that can be debited from a privilege
    /// </summary>
    /// <param name="privilegeId">The unique identifier of the Privilege</param>
    /// <returns>Maximum debit amount with HTTP 200 OK</returns>
    /// <response code="200">Returns the maximum debit amount</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Privilege not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("{privilegeId}/max-debit")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpPrivilegeNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpPrivilegeValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> GetMaxDebitAmount(int privilegeId)
    {
        try
        {
            _logger.LogDebug("Getting max debit amount for privilege: {PrivilegeId}", privilegeId);
            
            var maxAmount = await _privilegeService.GetMaxDebitAmountAsync(privilegeId);
            
            _logger.LogInformation("Max debit amount retrieved: {PrivilegeId}, Amount: {Amount}", privilegeId, maxAmount);
            return Ok(maxAmount);
        }
        catch (ServicePrivilegeNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege not found for max debit: {PrivilegeId}", privilegeId);
            return NotFound(new HttpPrivilegeNotFoundException(privilegeId));
        }
        catch (ServicePrivilegeValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege validation failed for max debit: {PrivilegeId}", privilegeId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting max debit amount for privilege: {PrivilegeId}", privilegeId);
            throw new PrivilegeInternalServerException(ex);
        }
    }
}
