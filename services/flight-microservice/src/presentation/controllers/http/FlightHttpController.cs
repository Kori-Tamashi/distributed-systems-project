using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Flight;
using presentation.exceptions.http;

using ServiceFlightNotFoundException = core.exceptions.businesslogic.services.FlightNotFoundException;
using ServiceFlightValidationException = core.exceptions.businesslogic.services.FlightValidationException;
using ServiceFlightBusinessRuleViolationException = core.exceptions.businesslogic.services.FlightBusinessRuleViolationException;

using HttpFlightNotFoundException = presentation.exceptions.http.FlightNotFoundException;
using HttpFlightValidationException = presentation.exceptions.http.FlightValidationException;
using HttpFlightBusinessRuleViolationException = presentation.exceptions.http.FlightBusinessRuleViolationException;

using HttpAirportNotFoundException = presentation.exceptions.http.AirportNotFoundException;

using System.Linq;

namespace presentation.controllers.http;

/// <summary>
/// HTTP Controller for Flight CRUD operations
/// Implements RESTful API endpoints for managing Flight entities
/// </summary>
[ApiController]
[Route("api/v1/flights")]
[Produces("application/json")]
[Consumes("application/json")]
public class FlightHttpController : ControllerBase
{
    private readonly IFlightService _flightService;
    private readonly ILogger<FlightHttpController> _logger;

    /// <summary>
    /// Initializes a new instance of the FlightHttpController
    /// </summary>
    /// <param name="flightService">The Flight business logic service</param>
    /// <param name="logger">The logger for the controller</param>
    public FlightHttpController(
        IFlightService flightService,
        ILogger<FlightHttpController> logger)
    {
        _flightService = flightService ?? throw new ArgumentNullException(nameof(flightService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a Flight by their unique identifier
    /// </summary>
    /// <param name="flightId">The unique identifier of the Flight</param>
    /// <returns>Flight data with HTTP 200 OK</returns>
    /// <response code="200">Returns the Flight</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Flight not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("{flightId}")]
    [ProducesResponseType(typeof(FlightDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpFlightNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpFlightValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FlightDTO>> GetFlightById(int flightId)
    {
        try
        {
            _logger.LogDebug("Getting flight by ID: {FlightId}", flightId);
            
            var flight = await _flightService.GetByIdAsync(flightId);
            var dto = FlightHttpConverter.ToDTO(flight);
            
            _logger.LogInformation("Flight retrieved successfully: {FlightId}", flightId);
            return Ok(dto);
        }
        catch (ServiceFlightNotFoundException ex)
        {
            _logger.LogWarning(ex, "Flight not found: {FlightId}", flightId);
            return NotFound(new HttpFlightNotFoundException(flightId));
        }
        catch (ServiceFlightValidationException ex)
        {
            _logger.LogWarning(ex, "Flight validation failed for ID: {FlightId}", flightId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpFlightValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flight by ID: {FlightId}", flightId);
            throw new FlightInternalServerException(ex);
        }
    }

    /// <summary>
    /// Gets all Flights
    /// </summary>
    /// <returns>List of all Flights with HTTP 200 OK</returns>
    /// <response code="200">Returns the list of Flights</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<FlightDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<FlightDTO>>> GetAllFlights(
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        try
        {
            _logger.LogDebug("Getting all flights");
            
            var flights = await _flightService.GetAllAsync();
            var totalCount = flights.Count;
            
            // Apply pagination if requested
            if (page.HasValue && pageSize.HasValue && pageSize.Value > 0)
            {
                flights = flights.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            }
            
            var dtos = FlightHttpConverter.ToDTO(flights);
            
            _logger.LogInformation("Retrieved {Count} flights", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all flights");
            throw new FlightInternalServerException(ex);
        }
    }

    /// <summary>
    /// Creates a new Flight
    /// </summary>
    /// <param name="createDto">The Flight data to create</param>
    /// <returns>Created Flight with HTTP 201 Created and Location header</returns>
    /// <response code="201">Flight created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Airport not found</response>
    /// <response code="409">Flight already exists</response>
    /// <response code="500">Server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(FlightDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpFlightValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpAirportNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpFlightBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FlightDTO>> CreateFlight(
        [FromBody] CreateFlightDTO createDto)
    {
        try
        {
            _logger.LogDebug("Creating new flight");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for flight creation");
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
                
                return BadRequest(new HttpFlightValidationException(validationErrors));
            }
            
            // Convert DTO to domain entity
            var flight = FlightHttpConverter.ToCreateDomain(createDto);
            
            // Create the flight
            var createdFlight = await _flightService.CreateAsync(flight);
            
            // Build location URI
            var location = Url.Action(nameof(GetFlightById), new { flightId = createdFlight.Id });
            var dto = FlightHttpConverter.ToDTO(createdFlight);
            
            _logger.LogInformation("Flight created successfully with ID: {FlightId}", createdFlight.Id);
            return CreatedAtAction(nameof(GetFlightById), new { flightId = createdFlight.Id }, dto);
        }
        catch (ServiceFlightValidationException ex)
        {
            _logger.LogWarning(ex, "Flight validation failed");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpFlightValidationException(errorData));
        }
        catch (HttpAirportNotFoundException ex)
        {
            _logger.LogWarning(ex, "Airport not found for flight creation: {AirportId}", ex.AirportId);
            return NotFound(new HttpAirportNotFoundException(ex.AirportId));
        }
        catch (ServiceFlightBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Flight business rule violated");
            return Conflict(new HttpFlightBusinessRuleViolationException(ex.RuleName, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating flight: {Error}", ex.Message);
            throw new FlightInternalServerException(ex);
        }
    }

    /// <summary>
    /// Updates an existing Flight
    /// </summary>
    /// <param name="flightId">The unique identifier of the Flight to update</param>
    /// <param name="updateDto">The updated Flight data</param>
    /// <returns>Updated Flight with HTTP 200 OK</returns>
    /// <response code="200">Flight updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Flight not found</response>
    /// <response code="409">Flight already exists</response>
    /// <response code="500">Server error</response>
    [HttpPatch("{flightId}")]
    [ProducesResponseType(typeof(FlightDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpFlightValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpFlightNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpFlightBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FlightDTO>> UpdateFlight(
        int flightId,
        [FromBody] UpdateFlightDTO updateDto)
    {
        try
        {
            _logger.LogDebug("Updating flight with ID: {FlightId}", flightId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for flight update");
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
                
                return BadRequest(new HttpFlightValidationException(validationErrors));
            }
            
            // Ensure IDs match
            if (flightId != updateDto.Id)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} != body ID {BodyId}", flightId, updateDto.Id);
                return BadRequest(new HttpFlightValidationException("Id", "ID in URL must match ID in request body"));
            }
            
            // Convert DTO to domain entity (partial update)
            var existingFlight = await _flightService.GetByIdAsync(flightId);
            var flight = FlightHttpConverter.ToUpdateDomain(updateDto, existingFlight);
            
            // Update the flight
            var updatedFlight = await _flightService.UpdateAsync(flight);
            var dto = FlightHttpConverter.ToDTO(updatedFlight);
            
            _logger.LogInformation("Flight updated successfully: {FlightId}", flightId);
            return Ok(dto);
        }
        catch (ServiceFlightNotFoundException ex)
        {
            _logger.LogWarning(ex, "Flight not found for update: {FlightId}", flightId);
            return NotFound(new HttpFlightNotFoundException(flightId));
        }
        catch (ServiceFlightValidationException ex)
        {
            _logger.LogWarning(ex, "Flight validation failed during update");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpFlightValidationException(errorData));
        }
        catch (HttpAirportNotFoundException ex)
        {
            _logger.LogWarning(ex, "Airport not found during flight update: {AirportId}", ex.AirportId);
            return NotFound(new HttpAirportNotFoundException(ex.AirportId));
        }
        catch (ServiceFlightBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Flight business rule violated during update");
            return Conflict(new HttpFlightBusinessRuleViolationException(ex.RuleName, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight: {FlightId}", flightId);
            throw new FlightInternalServerException(ex);
        }
    }

    /// <summary>
    /// Deletes a Flight by their unique identifier
    /// </summary>
    /// <param name="flightId">The unique identifier of the Flight to delete</param>
    /// <returns>HTTP 200 OK with confirmation</returns>
    /// <response code="200">Flight deleted successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Flight not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{flightId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpFlightNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpFlightValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteFlight(int flightId)
    {
        try
        {
            _logger.LogDebug("Deleting flight with ID: {FlightId}", flightId);
            
            await _flightService.DeleteAsync(flightId);
            
            _logger.LogInformation("Flight deleted successfully: {FlightId}", flightId);
            return Ok(new { message = $"Flight with ID {flightId} was deleted successfully" });
        }
        catch (ServiceFlightNotFoundException ex)
        {
            _logger.LogWarning(ex, "Flight not found for deletion: {FlightId}", flightId);
            return NotFound(new HttpFlightNotFoundException(flightId));
        }
        catch (ServiceFlightValidationException ex)
        {
            _logger.LogWarning(ex, "Flight validation failed for ID: {FlightId}", flightId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpFlightValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting flight: {FlightId} - {Error}", flightId, ex.Message);
            throw new FlightInternalServerException(ex);
        }
    }
}
