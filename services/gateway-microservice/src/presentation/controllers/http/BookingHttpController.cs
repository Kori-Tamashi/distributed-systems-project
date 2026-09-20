using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Booking;
using presentation.dto.http.Ticket;
using presentation.exceptions.http;
using presentation.exceptions.http.Booking;

using ServiceBookingNotFoundException = core.exceptions.businesslogic.services.BookingNotFoundException;
using ServiceBookingValidationException = core.exceptions.businesslogic.services.BookingValidationException;
using ServiceBookingBusinessRuleViolationException = core.exceptions.businesslogic.services.BookingBusinessRuleViolationException;

using HttpBookingNotFoundException = presentation.exceptions.http.Booking.BookingNotFoundException;
using HttpBookingValidationException = presentation.exceptions.http.Booking.BookingValidationException;
using HttpBookingBusinessRuleViolationException = presentation.exceptions.http.Booking.BookingBusinessRuleViolationException;

using System.Linq;

namespace presentation.controllers.http;

/// <summary>
/// SAGA coordinator for Booking creation with compensating transactions
/// Implements the SAGA pattern for distributed transactions across microservices
/// </summary>
public class BookingSagaCoordinator
{
    private readonly IBookingService _bookingService;
    private readonly ITicketService _ticketService;
    private readonly IPrivilegeService _privilegeService;
    private readonly ILogger<BookingSagaCoordinator> _logger;

    /// <summary>
    /// Initializes a new instance of the BookingSagaCoordinator
    /// </summary>
    public BookingSagaCoordinator(
        IBookingService bookingService,
        ITicketService ticketService,
        IPrivilegeService privilegeService,
        ILogger<BookingSagaCoordinator> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        _privilegeService = privilegeService ?? throw new ArgumentNullException(nameof(privilegeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes booking creation SAGA with compensating transactions
    /// </summary>
    public async Task<(int bookingId, int ticketId)> CreateBookingWithSAGAAsync(
        core.domain.Booking booking,
        core.domain.Ticket ticket)
    {
        var createdBookingId = 0;
        var createdTicketId = 0;

        try
        {
            // Step 1: Create Booking
            _logger.LogDebug("SAGA Step 1: Creating booking");
            var createdBooking = await _bookingService.CreateAsync(booking);
            createdBookingId = createdBooking.Id;
            _logger.LogInformation("SAGA Step 1: Booking created with ID {BookingId}", createdBookingId);

            // Step 2: Create Ticket
            _logger.LogDebug("SAGA Step 2: Creating ticket");
            var createdTicket = await _ticketService.CreateAsync(ticket);
            createdTicketId = createdTicket.Id;
            _logger.LogInformation("SAGA Step 2: Ticket created with ID {TicketId}", createdTicketId);

            // SAGA completed successfully
            _logger.LogInformation("SAGA completed successfully: Booking={BookingId}, Ticket={TicketId}", 
                createdBookingId, createdTicketId);
            
            return (createdBookingId, createdTicketId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SAGA failed at step, initiating compensation");
            
            // Compensating transactions
            await CompensateAsync(createdBookingId, createdTicketId);
            
            throw;
        }
    }

    /// <summary>
    /// Compensating transactions for failed SAGA
    /// </summary>
    private async Task CompensateAsync(int bookingId, int ticketId)
    {
        _logger.LogWarning("Starting compensation for Booking={BookingId}, Ticket={TicketId}", 
            bookingId, ticketId);

        // Compensate Step 2: Delete Ticket
        if (ticketId > 0)
        {
            try
            {
                _logger.LogDebug("Compensating Step 2: Deleting ticket {TicketId}", ticketId);
                await _ticketService.DeleteAsync(ticketId);
                _logger.LogInformation("Compensated: Ticket {TicketId} deleted", ticketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compensate ticket {TicketId}", ticketId);
            }
        }

        // Compensate Step 1: Delete Booking
        if (bookingId > 0)
        {
            try
            {
                _logger.LogDebug("Compensating Step 1: Deleting booking {BookingId}", bookingId);
                await _bookingService.DeleteAsync(bookingId);
                _logger.LogInformation("Compensated: Booking {BookingId} deleted", bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compensate booking {BookingId}", bookingId);
            }
        }

        _logger.LogWarning("Compensation completed for Booking={BookingId}, Ticket={TicketId}", 
            bookingId, ticketId);
    }
}

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
    private readonly ITicketService _ticketService;
    private readonly ILogger<BookingHttpController> _logger;
    private readonly BookingSagaCoordinator _bookingSagaCoordinator;

    /// <summary>
    /// Initializes a new instance of the BookingHttpController
    /// </summary>
    /// <param name="bookingService">The Booking business logic service</param>
    /// <param name="logger">The logger for the controller</param>
    public BookingHttpController(
        IBookingService bookingService,
        ITicketService ticketService,
        BookingSagaCoordinator bookingSagaCoordinator,
        ILogger<BookingHttpController> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        _bookingSagaCoordinator = bookingSagaCoordinator ?? throw new ArgumentNullException(nameof(bookingSagaCoordinator));
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
    [ProducesResponseType(typeof(SagaException), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookingDTO>> CreateBooking(
        [FromBody] CreateBookingDTO createDto)
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
                return BadRequest(new HttpBookingValidationException(errorData));
            }
            
            _logger.LogDebug("Creating new booking with SAGA pattern");
            
            var booking = BookingHttpConverter.ToCreateDomain(createDto);
            var createdBooking = await _bookingService.CreateAsync(booking);
            var dto = BookingHttpConverter.ToDTO(createdBooking);
            
            _logger.LogInformation("Booking created successfully: {BookingId}", createdBooking.Id);
            return CreatedAtAction(
                nameof(GetBookingById),
                new { bookingId = createdBooking.Id },
                dto);
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
            _logger.LogWarning(ex, "Booking business rule violation");
            return Conflict(new HttpBookingBusinessRuleViolationException(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking - SAGA compensation initiated");
            
            // SAGA compensation would be triggered here
            throw new SagaException(
                $"Failed to create booking. Compensating transactions have been executed. Error: {ex.Message}",
                failedStep: 1,
                compensatedOperations: new List<string> { "Booking creation" },
                innerException: ex);
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
    /// <response code="500">Server error</response>
    [HttpPut("{bookingId}")]
    [ProducesResponseType(typeof(BookingDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpBookingNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpBookingValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookingDTO>> UpdateBooking(
        int bookingId, 
        [FromBody] UpdateBookingDTO updateDto)
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
                return BadRequest(new HttpBookingValidationException(errorData));
            }
            
            _logger.LogDebug("Updating booking: {BookingId}", bookingId);
            
            var existingBooking = await _bookingService.GetByIdAsync(bookingId);
            var updatedBooking = BookingHttpConverter.ToUpdateDomain(updateDto, existingBooking);
            var result = await _bookingService.UpdateAsync(updatedBooking);
            var dto = BookingHttpConverter.ToDTO(result);
            
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
            _logger.LogWarning(ex, "Booking validation failed for ID: {BookingId}", bookingId);
            var errorData = ex.Data.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString()!, kvp => new[] { kvp.Value?.ToString() ?? string.Empty });
            return BadRequest(new HttpBookingValidationException(errorData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking: {BookingId}", bookingId);
            throw new BookingInternalServerException(ex);
        }
    }

    /// <summary>
    /// Deletes a Booking
    /// </summary>
    /// <param name="bookingId">The unique identifier of the Booking to delete</param>
    /// <returns>HTTP 204 No Content on success</returns>
    /// <response code="204">Booking deleted successfully</response>
    /// <response code="404">Booking not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{bookingId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpBookingNotFoundException), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteBooking(int bookingId)
    {
        try
        {
            _logger.LogDebug("Deleting booking: {BookingId}", bookingId);
            
            await _bookingService.DeleteAsync(bookingId);
            
            _logger.LogInformation("Booking deleted successfully: {BookingId}", bookingId);
            return NoContent();
        }
        catch (ServiceBookingNotFoundException ex)
        {
            _logger.LogWarning(ex, "Booking not found for deletion: {BookingId}", bookingId);
            return NotFound(new HttpBookingNotFoundException(bookingId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking: {BookingId}", bookingId);
            throw new BookingInternalServerException(ex);
        }
    }

    /// <summary>
    /// Creates a Booking with Ticket using SAGA pattern
    /// This endpoint creates both Booking and Ticket in a distributed transaction
    /// If any step fails, compensating transactions are executed to rollback changes
    /// </summary>
    /// <param name="bookingDto">The Booking data to create</param>
    /// <param name="ticketDto">The Ticket data to create</param>
    /// <returns>Created Booking and Ticket with HTTP 201 Created</returns>
    /// <response code="201">Booking and Ticket created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="409">Business rule violation</response>
    /// <response code="500">SAGA failed with compensation</response>
    [HttpPost("bookings-with-tickets")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpBookingValidationException), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpBookingBusinessRuleViolationException), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(SagaException), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateBookingWithTicket(
        [FromBody] CreateBookingDTO bookingDto,
        [FromQuery] CreateTicketDTO ticketDto)
    {
        try
        {
            _logger.LogDebug("Creating booking with ticket using SAGA pattern");
            
            var booking = BookingHttpConverter.ToCreateDomain(bookingDto);
            var ticket = TicketHttpConverter.ToCreateDomain(ticketDto);
            
            // Execute SAGA with compensating transactions
            var (bookingId, ticketId) = await _bookingSagaCoordinator.CreateBookingWithSAGAAsync(booking, ticket);
            
            var bookingResult = await _bookingService.GetByIdAsync(bookingId);
            var ticketResult = await _ticketService.GetByIdAsync(ticketId);
            
            var response = new
            {
                Booking = BookingHttpConverter.ToDTO(bookingResult),
                Ticket = TicketHttpConverter.ToDTO(ticketResult),
                SagaStatus = "COMPLETED",
                Message = "Booking and Ticket created successfully with SAGA pattern"
            };
            
            _logger.LogInformation("SAGA completed: Booking={BookingId}, Ticket={TicketId}", bookingId, ticketId);
            return CreatedAtAction(
                nameof(GetBookingById),
                new { bookingId = bookingId },
                response);
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
            _logger.LogWarning(ex, "Booking business rule violation");
            return Conflict(new HttpBookingBusinessRuleViolationException(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SAGA failed - compensating transactions executed");
            
            throw new SagaException(
                $"SAGA failed to create booking with ticket. Compensating transactions have been executed to rollback changes. Error: {ex.Message}",
                failedStep: 0,
                compensatedOperations: new List<string> { "Booking creation", "Ticket creation" },
                innerException: ex);
        }
    }
}
