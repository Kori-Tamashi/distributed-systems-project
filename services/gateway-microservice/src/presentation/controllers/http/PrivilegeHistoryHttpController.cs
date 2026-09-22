using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.PrivilegeHistory;
using presentation.exceptions.http;
using presentation.exceptions.http.PrivilegeHistory;

using ServicePrivilegeHistoryNotFoundException = core.exceptions.businesslogic.services.PrivilegeHistoryNotFoundException;
using ServicePrivilegeHistoryValidationException = core.exceptions.businesslogic.services.PrivilegeHistoryValidationException;

using HttpPrivilegeHistoryNotFoundException = presentation.exceptions.http.PrivilegeHistory.PrivilegeHistoryNotFoundException;
using HttpPrivilegeHistoryValidationException = presentation.exceptions.http.PrivilegeHistory.PrivilegeHistoryValidationException;

using System.Linq;

namespace presentation.controllers.http;

/// <summary>
/// HTTP Controller for PrivilegeHistory CRUD operations
/// Implements RESTful API endpoints for managing PrivilegeHistory entities
/// </summary>
[ApiController]
[Route("api/v1/privilege-history")]
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
            
            var history = await _privilegeHistoryService.GetByIdAsync(privilegeHistoryId);
            var dto = PrivilegeHistoryHttpConverter.ToDTO(history);
            
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
    /// Gets all PrivilegeHistories
    /// </summary>
    /// <returns>List of all PrivilegeHistories with HTTP 200 OK</returns>
    /// <response code="200">Returns the list of PrivilegeHistories</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<PrivilegeHistoryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<PrivilegeHistoryDTO>>> GetAllPrivilegeHistories(
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        try
        {
            _logger.LogDebug("Getting all privilege histories");
            
            var histories = await _privilegeHistoryService.GetAllAsync();
            var totalCount = histories.Count;
            
            // Apply pagination if requested
            if (page.HasValue && pageSize.HasValue && pageSize.Value > 0)
            {
                histories = histories.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            }
            
            var dtos = PrivilegeHistoryHttpConverter.ToDTO(histories);
            
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
    /// <response code="500">Server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(PrivilegeHistoryDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpPrivilegeHistoryValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrivilegeHistoryDTO>> CreatePrivilegeHistory(
        [FromBody] CreatePrivilegeHistoryDTO createDto)
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
                return BadRequest(new HttpPrivilegeHistoryValidationException(errorData));
            }
            
            _logger.LogDebug("Creating new privilege history");
            
            var history = PrivilegeHistoryHttpConverter.ToCreateDomain(createDto);
            var createdHistory = await _privilegeHistoryService.CreateAsync(history);
            var dto = PrivilegeHistoryHttpConverter.ToDTO(createdHistory);
            
            _logger.LogInformation("Privilege history created successfully: {PrivilegeHistoryId}", createdHistory.Id);
            return CreatedAtAction(
                nameof(GetPrivilegeHistoryById),
                new { privilegeHistoryId = createdHistory.Id },
                dto);
        }
        catch (ServicePrivilegeHistoryValidationException ex)
        {
            _logger.LogWarning(ex, "Privilege history validation failed");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpPrivilegeHistoryValidationException(errorData));
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
    /// <param name="privilegeHistoryId">The unique identifier of the PrivilegeHistory to update</param>
    /// <param name="updateDto">The updated PrivilegeHistory data</param>
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
            if (!ModelState.IsValid)
            {
                var errorData = ModelState
                    .Where(x => x.Value != null && x.Value!.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new HttpPrivilegeHistoryValidationException(errorData));
            }
            
            _logger.LogDebug("Updating privilege history: {PrivilegeHistoryId}", privilegeHistoryId);
            
            var existingHistory = await _privilegeHistoryService.GetByIdAsync(privilegeHistoryId);
            
            // Ensure IDs match
            if (existingHistory != null && existingHistory.Id != privilegeHistoryId)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} != entity ID {EntityId}", privilegeHistoryId, existingHistory.Id);
                var errorData = new Dictionary<string, string[]> { { "id", new[] { "Route ID must match entity ID" } } };
                return BadRequest(new HttpPrivilegeHistoryValidationException(errorData));
            }
            
            var updatedHistory = PrivilegeHistoryHttpConverter.ToUpdateDomain(updateDto, existingHistory);
            var result = await _privilegeHistoryService.UpdateAsync(updatedHistory);
            var dto = PrivilegeHistoryHttpConverter.ToDTO(result);
            
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
            _logger.LogError(ex, "Error updating privilege history: {PrivilegeHistoryId}", privilegeHistoryId);
            throw new PrivilegeHistoryInternalServerException(ex);
        }
    }

    /// <summary>
    /// Deletes a PrivilegeHistory
    /// </summary>
    /// <param name="privilegeHistoryId">The unique identifier of the PrivilegeHistory to delete</param>
    /// <returns>HTTP 204 No Content on success</returns>
    /// <response code="204">PrivilegeHistory deleted successfully</response>
    /// <response code="404">PrivilegeHistory not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{privilegeHistoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpPrivilegeHistoryNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeletePrivilegeHistory(int privilegeHistoryId)
    {
        try
        {
            _logger.LogDebug("Deleting privilege history: {PrivilegeHistoryId}", privilegeHistoryId);
            
            await _privilegeHistoryService.DeleteAsync(privilegeHistoryId);
            
            _logger.LogInformation("Privilege history deleted successfully: {PrivilegeHistoryId}", privilegeHistoryId);
            return NoContent();
        }
        catch (ServicePrivilegeHistoryNotFoundException ex)
        {
            _logger.LogWarning(ex, "Privilege history not found for deletion: {PrivilegeHistoryId}", privilegeHistoryId);
            return NotFound(new HttpPrivilegeHistoryNotFoundException(privilegeHistoryId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting privilege history: {PrivilegeHistoryId}", privilegeHistoryId);
            throw new PrivilegeHistoryInternalServerException(ex);
        }
    }
}
