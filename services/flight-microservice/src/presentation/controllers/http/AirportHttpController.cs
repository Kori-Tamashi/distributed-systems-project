using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http.Airport;
using presentation.exceptions.http;

using ServiceAirportNotFoundException = core.exceptions.businesslogic.services.AirportNotFoundException;
using ServiceAirportValidationException = core.exceptions.businesslogic.services.AirportValidationException;
using ServiceAirportBusinessRuleViolationException = core.exceptions.businesslogic.services.AirportBusinessRuleViolationException;

using HttpAirportNotFoundException = presentation.exceptions.http.AirportNotFoundException;
using HttpAirportValidationException = presentation.exceptions.http.AirportValidationException;
using HttpAirportBusinessRuleViolationException = presentation.exceptions.http.AirportBusinessRuleViolationException;

using System.Linq;

namespace presentation.controllers.http;

/// <summary>
/// HTTP Controller for Airport CRUD operations
/// Implements RESTful API endpoints for managing Airport entities
/// </summary>
[ApiController]
[Route("api/v1/airports")]
[Produces("application/json")]
[Consumes("application/json")]
public class AirportHttpController : ControllerBase
{
    private readonly IAirportService _airportService;
    private readonly ILogger<AirportHttpController> _logger;

    /// <summary>
    /// Initializes a new instance of the AirportHttpController
    /// </summary>
    /// <param name="airportService">The Airport business logic service</param>
    /// <param name="logger">The logger for the controller</param>
    public AirportHttpController(
        IAirportService airportService,
        ILogger<AirportHttpController> logger)
    {
        _airportService = airportService ?? throw new ArgumentNullException(nameof(airportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets an Airport by their unique identifier
    /// </summary>
    /// <param name="airportId">The unique identifier of the Airport</param>
    /// <returns>Airport data with HTTP 200 OK</returns>
    /// <response code="200">Returns the Airport</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Airport not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("{airportId}")]
    [ProducesResponseType(typeof(AirportDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpAirportNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpAirportValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AirportDTO>> GetAirportById(int airportId)
    {
        try
        {
            _logger.LogDebug("Getting airport by ID: {AirportId}", airportId);
            
            var airport = await _airportService.GetByIdAsync(airportId);
            var dto = AirportHttpConverter.ToDTO(airport);
            
            _logger.LogInformation("Airport retrieved successfully: {AirportId}", airportId);
            return Ok(dto);
        }
        catch (ServiceAirportNotFoundException ex)
        {
            _logger.LogWarning(ex, "Airport not found: {AirportId}", airportId);
            return NotFound(new HttpAirportNotFoundException(airportId));
        }
        catch (ServiceAirportValidationException ex)
        {
            _logger.LogWarning(ex, "Airport validation failed for ID: {AirportId}", airportId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpAirportValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting airport by ID: {AirportId}", airportId);
            throw new AirportInternalServerException(ex);
        }
    }

    /// <summary>
    /// Gets all Airports
    /// </summary>
    /// <returns>List of all Airports with HTTP 200 OK</returns>
    /// <response code="200">Returns the list of Airports</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AirportDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AirportDTO>>> GetAllAirports(
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        try
        {
            _logger.LogDebug("Getting all airports");
            
            var airports = await _airportService.GetAllAsync();
            var totalCount = airports.Count;
            
            // Apply pagination if requested
            if (page.HasValue && pageSize.HasValue && pageSize.Value > 0)
            {
                airports = airports.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            }
            
            var dtos = AirportHttpConverter.ToDTO(airports);
            
            _logger.LogInformation("Retrieved {Count} airports", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all airports");
            throw new AirportInternalServerException(ex);
        }
    }

    /// <summary>
    /// Creates a new Airport
    /// </summary>
    /// <param name="createDto">The Airport data to create</param>
    /// <returns>Created Airport with HTTP 201 Created and Location header</returns>
    /// <response code="201">Airport created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="409">Airport already exists</response>
    /// <response code="500">Server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(AirportDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpAirportValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpAirportBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AirportDTO>> CreateAirport(
        [FromBody] CreateAirportDTO createDto)
    {
        try
        {
            _logger.LogDebug("Creating new airport");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for airport creation");
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
                
                return BadRequest(new HttpAirportValidationException(validationErrors));
            }
            
            // Convert DTO to domain entity
            var airport = AirportHttpConverter.ToCreateDomain(createDto);
            
            // Create the airport
            var createdAirport = await _airportService.CreateAsync(airport);
            
            // Build location URI
            var location = Url.Action(nameof(GetAirportById), new { airportId = createdAirport.Id });
            var dto = AirportHttpConverter.ToDTO(createdAirport);
            
            _logger.LogInformation("Airport created successfully with ID: {AirportId}", createdAirport.Id);
            return CreatedAtAction(nameof(GetAirportById), new { airportId = createdAirport.Id }, dto);
        }
        catch (ServiceAirportValidationException ex)
        {
            _logger.LogWarning(ex, "Airport validation failed");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpAirportValidationException(errorData));
        }
        catch (ServiceAirportBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Airport business rule violated");
            return Conflict(new HttpAirportBusinessRuleViolationException(ex.RuleName, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating airport: {Error}", ex.Message);
            throw new AirportInternalServerException(ex);
        }
    }

    /// <summary>
    /// Updates an existing Airport
    /// </summary>
    /// <param name="airportId">The unique identifier of the Airport to update</param>
    /// <param name="updateDto">The updated Airport data</param>
    /// <returns>Updated Airport with HTTP 200 OK</returns>
    /// <response code="200">Airport updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Airport not found</response>
    /// <response code="409">Airport already exists</response>
    /// <response code="500">Server error</response>
    [HttpPatch("{airportId}")]
    [ProducesResponseType(typeof(AirportDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpAirportValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpAirportNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpAirportBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AirportDTO>> UpdateAirport(
        int airportId,
        [FromBody] UpdateAirportDTO updateDto)
    {
        try
        {
            _logger.LogDebug("Updating airport with ID: {AirportId}", airportId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for airport update");
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
                
                return BadRequest(new HttpAirportValidationException(validationErrors));
            }
            
            // Ensure IDs match
            if (airportId != updateDto.Id)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} != body ID {BodyId}", airportId, updateDto.Id);
                return BadRequest(new HttpAirportValidationException("Id", "ID in URL must match ID in request body"));
            }
            
            // Convert DTO to domain entity (partial update)
            var existingAirport = await _airportService.GetByIdAsync(airportId);
            var airport = AirportHttpConverter.ToUpdateDomain(updateDto, existingAirport);
            
            // Update the airport
            var updatedAirport = await _airportService.UpdateAsync(airport);
            var dto = AirportHttpConverter.ToDTO(updatedAirport);
            
            _logger.LogInformation("Airport updated successfully: {AirportId}", airportId);
            return Ok(dto);
        }
        catch (ServiceAirportNotFoundException ex)
        {
            _logger.LogWarning(ex, "Airport not found for update: {AirportId}", airportId);
            return NotFound(new HttpAirportNotFoundException(airportId));
        }
        catch (ServiceAirportValidationException ex)
        {
            _logger.LogWarning(ex, "Airport validation failed during update");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpAirportValidationException(errorData));
        }
        catch (ServiceAirportBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Airport business rule violated during update");
            return Conflict(new HttpAirportBusinessRuleViolationException(ex.RuleName, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating airport: {AirportId}", airportId);
            throw new AirportInternalServerException(ex);
        }
    }

    /// <summary>
    /// Deletes an Airport by their unique identifier
    /// </summary>
    /// <param name="airportId">The unique identifier of the Airport to delete</param>
    /// <returns>HTTP 200 OK with confirmation</returns>
    /// <response code="200">Airport deleted successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Airport not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{airportId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpAirportNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpAirportValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteAirport(int airportId)
    {
        try
        {
            _logger.LogDebug("Deleting airport with ID: {AirportId}", airportId);
            
            await _airportService.DeleteAsync(airportId);
            
            _logger.LogInformation("Airport deleted successfully: {AirportId}", airportId);
            return Ok(new { message = $"Airport with ID {airportId} was deleted successfully" });
        }
        catch (ServiceAirportNotFoundException ex)
        {
            _logger.LogWarning(ex, "Airport not found for deletion: {AirportId}", airportId);
            return NotFound(new HttpAirportNotFoundException(airportId));
        }
        catch (ServiceAirportValidationException ex)
        {
            _logger.LogWarning(ex, "Airport validation failed for ID: {AirportId}", airportId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpAirportValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting airport: {AirportId} - {Error}", airportId, ex.Message);
            throw new AirportInternalServerException(ex);
        }
    }
}
