using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Booking;
using presentation.exceptions.http;

using ServiceBookingNotFoundException = core.exceptions.businesslogic.services.BookingNotFoundException;
using ServiceBookingValidationException = core.exceptions.businesslogic.services.BookingValidationException;
using ServiceBookingBusinessRuleViolationException = core.exceptions.businesslogic.services.BookingBusinessRuleViolationException;

using HttpBookingNotFoundException = presentation.exceptions.http.BookingNotFoundException;
using HttpBookingValidationException = presentation.exceptions.http.BookingValidationException;
using HttpBookingBusinessRuleViolationException = presentation.exceptions.http.BookingBusinessRuleViolationException;

using System.Linq;

namespace presentation.controllers.http;

/// <summary>
/// HTTP Controller for Booking CRUD operations
/// Implements RESTful API endpoints for managing Booking entities
/// </summary>
[ApiController]
[Route("api/v1/bookings")]
[Produces("application/json")]
[Consumes("application/json")]
public class BookingHttpController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingHttpController> _logger;

    /// <summary>
    /// Initializes a new instance of the BookingHttpController
    /// </summary>
    /// <param name="bookingService">The Booking business logic service</param>
    /// <param name="logger">The logger for the controller</param>
    public BookingHttpController(
        IBookingService bookingService,
        ILogger<BookingHttpController> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a Booking by their unique identifier
    /// </summary>
    /// <param name="bookingId">The unique identifier of the Booking</param>
    /// <returns>Booking data with HTTP 200 OK</returns>
    /// <response code="200">Returns the Booking</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Booking not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("{bookingId}")]
    [ProducesResponseType(typeof(BookingDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpBookingNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpBookingValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookingDTO>> GetBookingById(int bookingId)
    {
        try
        {
            _logger.LogDebug("Getting booking by ID: {BookingId}", bookingId);
            
            var booking = await _bookingService.GetByIdAsync(bookingId);
            var dto = BookingHttpConverter.ToDTO(booking);
            
            _logger.LogInformation("Booking retrieved successfully: {BookingId}", bookingId);
            return Ok(dto);
        }
        catch (ServiceBookingNotFoundException ex)
        {
            _logger.LogWarning(ex, "Booking not found: {BookingId}", bookingId);
            return NotFound(new HttpBookingNotFoundException(bookingId));
        }
        catch (ServiceBookingValidationException ex)
        {
            _logger.LogWarning(ex, "Booking validation failed for ID: {BookingId}", bookingId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpBookingValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking by ID: {BookingId}", bookingId);
            throw new BookingInternalServerException(ex);
        }
    }

    /// <summary>
    /// Gets all Bookings
    /// </summary>
    /// <returns>List of all Bookings with HTTP 200 OK</returns>
    /// <response code="200">Returns the list of Bookings</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<BookingDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<BookingDTO>>> GetAllBookings(
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        try
        {
            _logger.LogDebug("Getting all bookings");
            
            var bookings = await _bookingService.GetAllAsync();
            var totalCount = bookings.Count;
            
            // Apply pagination if requested
            if (page.HasValue && pageSize.HasValue && pageSize.Value > 0)
            {
                bookings = bookings.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            }
            
            var dtos = BookingHttpConverter.ToDTO(bookings);
            
            _logger.LogInformation("Retrieved {Count} bookings", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all bookings");
            throw new BookingInternalServerException(ex);
        }
    }

    /// <summary>
    /// Creates a new Booking
    /// </summary>
    /// <param name="createDto">The Booking data to create</param>
    /// <returns>Created Booking with HTTP 201 Created and Location header</returns>
    /// <response code="201">Booking created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="409">Booking already exists</response>
    /// <response code="500">Server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpBookingValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpBookingBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookingDTO>> CreateBooking(
        [FromBody] CreateBookingDTO createDto)
    {
        try
        {
            _logger.LogDebug("Creating new booking");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for booking creation");
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
                
                return BadRequest(new HttpBookingValidationException(validationErrors));
            }
            
            // Convert DTO to domain entity
            var booking = BookingHttpConverter.ToCreateDomain(createDto);
            
            // Create the booking
            var createdBooking = await _bookingService.CreateAsync(booking);
            
            // Build location URI
            var location = Url.Action(nameof(GetBookingById), new { bookingId = createdBooking.Id });
            var dto = BookingHttpConverter.ToDTO(createdBooking);
            
            _logger.LogInformation("Booking created successfully with ID: {BookingId}", createdBooking.Id);
            return CreatedAtAction(nameof(GetBookingById), new { bookingId = createdBooking.Id }, dto);
        }
        catch (ServiceBookingValidationException ex)
        {
            _logger.LogWarning(ex, "Booking validation failed");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpBookingValidationException(errorData));
        }
        catch (ServiceBookingBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Booking business rule violated");
            return Conflict(new HttpBookingBusinessRuleViolationException(ex.RuleName, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking: {Error}", ex.Message);
            throw new BookingInternalServerException(ex);
        }
    }

    /// <summary>
    /// Updates an existing Booking
    /// </summary>
    /// <param name="bookingId">The unique identifier of the Booking to update</param>
    /// <param name="updateDto">The updated Booking data</param>
    /// <returns>Updated Booking with HTTP 200 OK</returns>
    /// <response code="200">Booking updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Booking not found</response>
    /// <response code="409">Booking already exists</response>
    /// <response code="500">Server error</response>
    [HttpPut("{bookingId}")]
    [ProducesResponseType(typeof(BookingDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpBookingValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpBookingNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpBookingBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookingDTO>> UpdateBooking(
        int bookingId,
        [FromBody] UpdateBookingDTO updateDto)
    {
        try
        {
            _logger.LogDebug("Updating booking with ID: {BookingId}", bookingId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for booking update");
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
                
                return BadRequest(new HttpBookingValidationException(validationErrors));
            }
            
            // Ensure IDs match
            if (bookingId != updateDto.Id)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} != body ID {BodyId}", bookingId, updateDto.Id);
                return BadRequest(new HttpBookingValidationException("Id", "ID in URL must match ID in request body"));
            }
            
            // Convert DTO to domain entity (partial update)
            var existingBooking = await _bookingService.GetByIdAsync(bookingId);
            var booking = BookingHttpConverter.ToUpdateDomain(updateDto, existingBooking);
            
            // Update the booking
            var updatedBooking = await _bookingService.UpdateAsync(booking);
            var dto = BookingHttpConverter.ToDTO(updatedBooking);
            
            _logger.LogInformation("Booking updated successfully: {BookingId}", bookingId);
            return Ok(dto);
        }
        catch (ServiceBookingNotFoundException ex)
        {
            _logger.LogWarning(ex, "Booking not found for update: {BookingId}", bookingId);
            return NotFound(new HttpBookingNotFoundException(bookingId));
        }
        catch (ServiceBookingValidationException ex)
        {
            _logger.LogWarning(ex, "Booking validation failed during update");
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpBookingValidationException(errorData));
        }
        catch (ServiceBookingBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Booking business rule violated during update");
            return Conflict(new HttpBookingBusinessRuleViolationException(ex.RuleName, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking: {BookingId}", bookingId);
            throw new BookingInternalServerException(ex);
        }
    }

    /// <summary>
    /// Deletes a Booking by their unique identifier
    /// </summary>
    /// <param name="bookingId">The unique identifier of the Booking to delete</param>
    /// <returns>HTTP 200 OK with confirmation</returns>
    /// <response code="200">Booking deleted successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Booking not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{bookingId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpBookingNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpBookingValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteBooking(int bookingId)
    {
        try
        {
            _logger.LogDebug("Deleting booking with ID: {BookingId}", bookingId);
            
            await _bookingService.DeleteAsync(bookingId);
            
            _logger.LogInformation("Booking deleted successfully: {BookingId}", bookingId);
            return Ok(new { message = $"Booking with ID {bookingId} was deleted successfully" });
        }
        catch (ServiceBookingNotFoundException ex)
        {
            _logger.LogWarning(ex, "Booking not found for deletion: {BookingId}", bookingId);
            return NotFound(new HttpBookingNotFoundException(bookingId));
        }
        catch (ServiceBookingValidationException ex)
        {
            _logger.LogWarning(ex, "Booking validation failed for ID: {BookingId}", bookingId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpBookingValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking: {BookingId} - {Error}", bookingId, ex.Message);
            throw new BookingInternalServerException(ex);
        }
    }
}
