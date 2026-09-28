using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.PrivilegeHistory;
using presentation.exceptions.http;

using ServicePrivilegeHistoryNotFoundException = core.exceptions.businesslogic.services.PrivilegeHistoryNotFoundException;
using ServicePrivilegeHistoryValidationException = core.exceptions.businesslogic.services.PrivilegeHistoryValidationException;
using ServicePrivilegeHistoryBusinessRuleViolationException = core.exceptions.businesslogic.services.PrivilegeHistoryBusinessRuleViolationException;

using HttpPrivilegeHistoryNotFoundException = presentation.exceptions.http.PrivilegeHistoryNotFoundException;
using HttpPrivilegeHistoryValidationException = presentation.exceptions.http.PrivilegeHistoryValidationException;
using HttpPrivilegeHistoryBusinessRuleViolationException = presentation.exceptions.http.PrivilegeHistoryBusinessRuleViolationException;

using System.Linq;

namespace presentation.controllers.http;

/// <summary>
/// HTTP Controller for PrivilegeHistory CRUD operations
/// Implements RESTful API endpoints for managing PrivilegeHistory entities
/// </summary>
[ApiController]
[Route("api/v1/privilege-histories")]
[Produces("application/json")]
[Consumes("application/json")]
public class PrivilegeHistoryHttpController : ControllerBase
{
    private readonly IPrivilegeHistoryService _privilegeHistoryService;
    private readonly ILogger<PrivilegeHistoryHttpController> _logger;

    /// <summary>
    /// Initializes a new instance of the PrivilegeHistoryHttpController
    /// </summary>
    /// <param name="privilegeHistoryService">The PrivilegeHistory business logic service</param>
    /// <param name="logger">The logger for the controller</param>
    public PrivilegeHistoryHttpController(
        IPrivilegeHistoryService privilegeHistoryService,
        ILogger<PrivilegeHistoryHttpController> logger)
    {
        _privilegeHistoryService = privilegeHistoryService ?? throw new ArgumentNullException(nameof(privilegeHistoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a PrivilegeHistory by their unique identifier
    /// </summary>
    /// <param name="privilegeHistoryId">The unique identifier of the PrivilegeHistory</param>
    /// <returns>PrivilegeHistory data with HTTP 200 OK</returns>
    /// <response code="200">Returns the PrivilegeHistory</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">PrivilegeHistory not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("{privilegeHistoryId}")]
    [ProducesResponseType(typeof(PrivilegeHistoryDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpPrivilegeHistoryNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpPrivilegeHistoryValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrivilegeHistoryDTO>> GetPrivilegeHistoryById(int privilegeHistoryId)
    {
        try
        {
            _logger.LogDebug("Getting privilege history by ID: {PrivilegeHistoryId}", privilegeHistoryId);
            
            var privilegeHistory = await _privilegeHistoryService.GetByIdAsync(privilegeHistoryId);
            var dto = PrivilegeHistoryHttpConverter.ToDTO(privilegeHistory);
            
            _logger.LogInformation("Privilege history retrieved successfully: {PrivilegeHistoryId}", privilegeHistoryId);
            return Ok(dto);
        }
        catch (ServicePrivilegeHistoryNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege history not found: {PrivilegeHistoryId}", privilegeHistoryId);
            return NotFound(new HttpPrivilegeHistoryNotFoundException(privilegeHistoryId));
        }
        catch (ServicePrivilegeHistoryValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege history validation failed for ID: {PrivilegeHistoryId}", privilegeHistoryId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeHistoryValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting privilege history by ID: {PrivilegeHistoryId}", privilegeHistoryId);
            throw new PrivilegeHistoryInternalServerException(ex);
        }
    }

    /// <summary>
    /// Gets all PrivilegeHistory
    /// </summary>
    /// <returns>List of all PrivilegeHistory with HTTP 200 OK</returns>
    /// <response code="200">Returns the list of PrivilegeHistory</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<PrivilegeHistoryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<PrivilegeHistoryDTO>>> GetAllPrivilegeHistories(
        [FromQuery] PrivilegeHistoryFilter? filter,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        try
        {
            _logger.LogDebug("Getting all privilege histories");
            
            var privilegeHistories = await _privilegeHistoryService.GetAllAsync(filter);
            var totalCount = privilegeHistories.Count;
            
            // Apply pagination if requested
            if (page.HasValue && pageSize.HasValue && pageSize.Value > 0)
            {
                privilegeHistories = privilegeHistories.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            }
            
            var dtos = PrivilegeHistoryHttpConverter.ToDTO(privilegeHistories);
            
            _logger.LogInformation("Retrieved {Count} privilege histories", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all privilege histories");
            throw new PrivilegeHistoryInternalServerException(ex);
        }
    }

    /// <summary>
    /// Creates a new PrivilegeHistory
    /// </summary>
    /// <param name="createDto">The PrivilegeHistory data to create</param>
    /// <returns>Created PrivilegeHistory with HTTP 201 Created and Location header</returns>
    /// <response code="201">PrivilegeHistory created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="409">PrivilegeHistory already exists</response>
    /// <response code="500">Server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(PrivilegeHistoryDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpPrivilegeHistoryValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpPrivilegeHistoryBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrivilegeHistoryDTO>> CreatePrivilegeHistory(
        [FromBody] CreatePrivilegeHistoryDTO createDto)
    {
        try
        {
            // Validate ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Privilege history creation failed due to model state validation errors");
                var errorData = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new HttpPrivilegeHistoryValidationException(errorData));
            }
            
            _logger.LogDebug("Creating new privilege history");
            
            var privilegeHistory = PrivilegeHistoryHttpConverter.ToCreateDomain(createDto);
            var createdPrivilegeHistory = await _privilegeHistoryService.CreateAsync(privilegeHistory);
            var dto = PrivilegeHistoryHttpConverter.ToDTO(createdPrivilegeHistory);
            
            _logger.LogInformation("Privilege history created successfully: {PrivilegeHistoryId}", createdPrivilegeHistory.Id);
            return CreatedAtAction(
                nameof(GetPrivilegeHistoryById),
                new { privilegeHistoryId = createdPrivilegeHistory.Id },
                dto);
        }
        catch (ServicePrivilegeHistoryValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege history validation failed");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeHistoryValidationException(errorData));
        }
        catch (ServicePrivilegeHistoryBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Privilege history business rule violation");
            return Conflict(new HttpPrivilegeHistoryBusinessRuleViolationException(ex.RuleName, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating privilege history");
            throw new PrivilegeHistoryInternalServerException(ex);
        }
    }

    /// <summary>
    /// Updates an existing PrivilegeHistory
    /// </summary>
    /// <param name="privilegeHistoryId">The unique identifier of the PrivilegeHistory</param>
    /// <param name="updateDto">The PrivilegeHistory data to update</param>
    /// <returns>Updated PrivilegeHistory with HTTP 200 OK</returns>
    /// <response code="200">PrivilegeHistory updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">PrivilegeHistory not found</response>
    /// <response code="500">Server error</response>
    [HttpPut("{privilegeHistoryId}")]
    [ProducesResponseType(typeof(PrivilegeHistoryDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpPrivilegeHistoryNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpPrivilegeHistoryValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrivilegeHistoryDTO>> UpdatePrivilegeHistory(
        int privilegeHistoryId,
        [FromBody] UpdatePrivilegeHistoryDTO updateDto)
    {
        try
        {
            // Validate ID match
            if (privilegeHistoryId != updateDto.Id)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} != DTO ID {DtoId}", privilegeHistoryId, updateDto.Id);
                var errorData = new Dictionary<string, string[]> { { "Id", new[] { "Route ID and DTO ID must match" } } };
                return BadRequest(new HttpPrivilegeHistoryValidationException(errorData));
            }
            
            // Validate ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Privilege history update failed due to model state validation errors");
                var errorData = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new HttpPrivilegeHistoryValidationException(errorData));
            }
            
            _logger.LogDebug("Updating privilege history by ID: {PrivilegeHistoryId}", privilegeHistoryId);
            
            var existingPrivilegeHistory = await _privilegeHistoryService.GetByIdAsync(privilegeHistoryId);
            var updatedPrivilegeHistory = await _privilegeHistoryService.UpdateAsync(PrivilegeHistoryHttpConverter.ToUpdateDomain(updateDto, existingPrivilegeHistory));
            var dto = PrivilegeHistoryHttpConverter.ToDTO(updatedPrivilegeHistory);
            
            _logger.LogInformation("Privilege history updated successfully: {PrivilegeHistoryId}", privilegeHistoryId);
            return Ok(dto);
        }
        catch (ServicePrivilegeHistoryNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege history not found for update: {PrivilegeHistoryId}", privilegeHistoryId);
            return NotFound(new HttpPrivilegeHistoryNotFoundException(privilegeHistoryId));
        }
        catch (ServicePrivilegeHistoryValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege history validation failed for ID: {PrivilegeHistoryId}", privilegeHistoryId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeHistoryValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating privilege history by ID: {PrivilegeHistoryId}", privilegeHistoryId);
            throw new PrivilegeHistoryInternalServerException(ex);
        }
    }

    /// <summary>
    /// Deletes a PrivilegeHistory by their unique identifier
    /// </summary>
    /// <param name="privilegeHistoryId">The unique identifier of the PrivilegeHistory</param>
    /// <returns>HTTP 204 No Content on success</returns>
    /// <response code="204">PrivilegeHistory deleted successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">PrivilegeHistory not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{privilegeHistoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpPrivilegeHistoryNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpPrivilegeHistoryValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeletePrivilegeHistory(int privilegeHistoryId)
    {
        try
        {
            _logger.LogDebug("Deleting privilege history by ID: {PrivilegeHistoryId}", privilegeHistoryId);
            
            await _privilegeHistoryService.DeleteAsync(privilegeHistoryId);
            
            _logger.LogInformation("Privilege history deleted successfully: {PrivilegeHistoryId}", privilegeHistoryId);
            return NoContent();
        }
        catch (ServicePrivilegeHistoryNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege history not found for deletion: {PrivilegeHistoryId}", privilegeHistoryId);
            return NotFound(new HttpPrivilegeHistoryNotFoundException(privilegeHistoryId));
        }
        catch (ServicePrivilegeHistoryValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege history validation failed for ID: {PrivilegeHistoryId}", privilegeHistoryId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeHistoryValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting privilege history by ID: {PrivilegeHistoryId}", privilegeHistoryId);
            throw new PrivilegeHistoryInternalServerException(ex);
        }
    }
}
