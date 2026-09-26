using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Flight;
using presentation.exceptions.http;
using presentation.exceptions.http.Flight;

using ServiceFlightNotFoundException = core.exceptions.businesslogic.services.FlightNotFoundException;
using ServiceFlightValidationException = core.exceptions.businesslogic.services.FlightValidationException;

using HttpFlightNotFoundException = presentation.exceptions.http.Flight.FlightNotFoundException;
using HttpFlightValidationException = presentation.exceptions.http.Flight.FlightValidationException;

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
            throw new HttpFlightNotFoundException(flightId);
        }
        catch (ServiceFlightValidationException ex)
        {
            _logger.LogWarning(ex, "Flight validation failed for ID: {FlightId}", flightId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            throw new HttpFlightValidationException(errorData);
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
    [ProducesResponseType(typeof(PaginationResponse<FlightDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginationResponse<FlightDTO>>> GetAllFlights(
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        try
        {
            _logger.LogDebug("Getting all flights");
            
            var flights = await _flightService.GetAllAsync();
            var totalCount = flights.Count;
            
            // Apply pagination if requested
            var pagedFlights = flights;
            if (page.HasValue && pageSize.HasValue && pageSize.Value > 0)
            {
                pagedFlights = flights.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            }
            
            var dtos = FlightHttpConverter.ToDTO(pagedFlights);
            
            _logger.LogInformation("Retrieved {Count} flights (page {Page}, size {PageSize})", dtos.Count, page ?? 1, pageSize ?? 10);
            
            var response = new PaginationResponse<FlightDTO>
            {
                Items = dtos,
                Page = page ?? 1,
                PageSize = pageSize ?? 10,
                TotalElements = totalCount
            };
            
            return Ok(response);
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
    /// <response code="500">Server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(FlightDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpFlightValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FlightDTO>> CreateFlight(
        [FromBody] CreateFlightDTO createDto)
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
                throw new HttpFlightValidationException(errorData);
            }
            
            _logger.LogDebug("Creating new flight");
            
            var flight = FlightHttpConverter.ToCreateDomain(createDto);
            var createdFlight = await _flightService.CreateAsync(flight);
            var dto = FlightHttpConverter.ToDTO(createdFlight);
            
            _logger.LogInformation("Flight created successfully: {FlightId}", createdFlight.Id);
            return CreatedAtAction(
                nameof(GetFlightById),
                new { flightId = createdFlight.Id },
                dto);
        }
        catch (ServiceFlightValidationException ex)
        {
            _logger.LogWarning(ex, "Flight validation failed");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            throw new HttpFlightValidationException(errorData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating flight");
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
    /// <response code="500">Server error</response>
    [HttpPut("{flightId}")]
    [ProducesResponseType(typeof(FlightDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpFlightNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpFlightValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FlightDTO>> UpdateFlight(
        int flightId, 
        [FromBody] UpdateFlightDTO updateDto)
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
                throw new HttpFlightValidationException(errorData);
            }
            
            _logger.LogDebug("Updating flight: {FlightId}", flightId);
            
            var existingFlight = await _flightService.GetByIdAsync(flightId);
            
            // Ensure IDs match
            if (existingFlight != null && existingFlight.Id != flightId)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} != entity ID {EntityId}", flightId, existingFlight.Id);
                var errorData = new Dictionary<string, string[]> { { "id", new[] { "Route ID must match entity ID" } } };
                throw new HttpFlightValidationException(errorData);
            }
            
            var updatedFlight = FlightHttpConverter.ToUpdateDomain(updateDto, existingFlight);
            var result = await _flightService.UpdateAsync(updatedFlight);
            var dto = FlightHttpConverter.ToDTO(result);
            
            _logger.LogInformation("Flight updated successfully: {FlightId}", flightId);
            return Ok(dto);
        }
        catch (ServiceFlightNotFoundException ex)
        {
            _logger.LogWarning(ex, "Flight not found for update: {FlightId}", flightId);
            throw new HttpFlightNotFoundException(flightId);
        }
        catch (ServiceFlightValidationException ex)
        {
            _logger.LogWarning(ex, "Flight validation failed for ID: {FlightId}", flightId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            throw new HttpFlightValidationException(errorData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight: {FlightId}", flightId);
            throw new FlightInternalServerException(ex);
        }
    }

    /// <summary>
    /// Deletes a Flight
    /// </summary>
    /// <param name="flightId">The unique identifier of the Flight to delete</param>
    /// <returns>HTTP 204 No Content on success</returns>
    /// <response code="204">Flight deleted successfully</response>
    /// <response code="404">Flight not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{flightId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpFlightNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteFlight(int flightId)
    {
        try
        {
            _logger.LogDebug("Deleting flight: {FlightId}", flightId);
            
            await _flightService.DeleteAsync(flightId);
            
            _logger.LogInformation("Flight deleted successfully: {FlightId}", flightId);
            return NoContent();
        }
        catch (ServiceFlightNotFoundException ex)
        {
            _logger.LogWarning(ex, "Flight not found for deletion: {FlightId}", flightId);
            throw new HttpFlightNotFoundException(flightId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting flight: {FlightId}", flightId);
            throw new FlightInternalServerException(ex);
        }
    }
}
