using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Privilege;
using presentation.exceptions.http;
using presentation.exceptions.http.Privilege;

using ServicePrivilegeNotFoundException = core.exceptions.businesslogic.services.PrivilegeNotFoundException;
using ServicePrivilegeValidationException = core.exceptions.businesslogic.services.PrivilegeValidationException;

using HttpPrivilegeNotFoundException = presentation.exceptions.http.Privilege.PrivilegeNotFoundException;
using HttpPrivilegeValidationException = presentation.exceptions.http.Privilege.PrivilegeValidationException;

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
    /// <response code="500">Server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(PrivilegeDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpPrivilegeValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrivilegeDTO>> CreatePrivilege(
        [FromBody] CreatePrivilegeDTO createDto)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating privilege");
            throw new PrivilegeInternalServerException(ex);
        }
    }

    /// <summary>
    /// Updates an existing Privilege
    /// </summary>
    /// <param name="privilegeId">The unique identifier of the Privilege to update</param>
    /// <param name="updateDto">The updated Privilege data</param>
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
            if (!ModelState.IsValid)
            {
                var errorData = ModelState
                    .Where(x => x.Value != null && x.Value!.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new HttpPrivilegeValidationException(errorData));
            }
            
            _logger.LogDebug("Updating privilege: {PrivilegeId}", privilegeId);
            
            var existingPrivilege = await _privilegeService.GetByIdAsync(privilegeId);
            
            // Ensure IDs match
            if (existingPrivilege != null && existingPrivilege.Id != privilegeId)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} != entity ID {EntityId}", privilegeId, existingPrivilege.Id);
                var errorData = new Dictionary<string, string[]> { { "id", new[] { "Route ID must match entity ID" } } };
                return BadRequest(new HttpPrivilegeValidationException(errorData));
            }
            
            var updatedPrivilege = PrivilegeHttpConverter.ToUpdateDomain(updateDto, existingPrivilege);
            var result = await _privilegeService.UpdateAsync(updatedPrivilege);
            var dto = PrivilegeHttpConverter.ToDTO(result);
            
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
            _logger.LogError(ex, "Error updating privilege: {PrivilegeId}", privilegeId);
            throw new PrivilegeInternalServerException(ex);
        }
    }

    /// <summary>
    /// Deletes a Privilege
    /// </summary>
    /// <param name="privilegeId">The unique identifier of the Privilege to delete</param>
    /// <returns>HTTP 204 No Content on success</returns>
    /// <response code="204">Privilege deleted successfully</response>
    /// <response code="404">Privilege not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{privilegeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpPrivilegeNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeletePrivilege(int privilegeId)
    {
        try
        {
            _logger.LogDebug("Deleting privilege: {PrivilegeId}", privilegeId);
            
            await _privilegeService.DeleteAsync(privilegeId);
            
            _logger.LogInformation("Privilege deleted successfully: {PrivilegeId}", privilegeId);
            return NoContent();
        }
        catch (ServicePrivilegeNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege not found for deletion: {PrivilegeId}", privilegeId);
            return NotFound(new HttpPrivilegeNotFoundException(privilegeId));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error deleting privilege: {PrivilegeId}", privilegeId);
            return BadRequest(new { error = "Validation failed", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting privilege: {PrivilegeId}", privilegeId);
            throw new PrivilegeInternalServerException(ex);
        }
    }
}
