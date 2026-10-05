using Microsoft.AspNetCore.Authorization;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using Microsoft.AspNetCore.Mvc;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.User;
using presentation.exceptions.http;
using presentation.exceptions.http.User;

namespace presentation.controllers.http;

/// <summary>
/// HTTP Controller for User information aggregation
/// Provides endpoints to get complete user information including tickets and privilege status
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class UserHttpController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IPrivilegeService _privilegeService;
    private readonly IFlightGateway _flightGateway;
    private readonly ILogger<UserHttpController> _logger;

    /// <summary>
    /// Initializes a new instance of the UserHttpController
    /// </summary>
    /// <param name="ticketService">The Ticket business logic service</param>
    /// <param name="privilegeService">The Privilege business logic service</param>
    /// <param name="flightGateway">The Flight gateway for flight data</param>
    /// <param name="logger">The logger for the controller</param>
    public UserHttpController(
        ITicketService ticketService,
        IPrivilegeService privilegeService,
        IFlightGateway flightGateway,
        ILogger<UserHttpController> logger)
    {
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        _privilegeService = privilegeService ?? throw new ArgumentNullException(nameof(privilegeService));
        _flightGateway = flightGateway ?? throw new ArgumentNullException(nameof(flightGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets complete user information including tickets and privilege status
    /// Aggregates data from Ticket and Privilege services
    /// </summary>
    /// <returns>Complete user information with HTTP 200 OK</returns>
    /// <response code="200">Returns user information</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserInfoDTO>> GetUserInfo()
    {
        // Get username from JWT token
        var username = User.FindFirst("preferred_username")?.Value 
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        
        try
        {
            // Validate username
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Username not found in JWT token");
                return BadRequest(new ErrorResponse("Username is required"));
            }

            _logger.LogDebug("Getting user information for: {Username}", username);

            // Bonus is non-critical for /me: if unavailable, privilege = null
            core.domain.Privilege? privilege = null;
            try
            {
                var userPrivilege = await _privilegeService.GetAllAsync(new core.filters.PrivilegeFilter { Username = username });
                privilege = userPrivilege.FirstOrDefault();
                if (privilege == null)
                {
                    _logger.LogWarning("User not found: {Username}", username);
                    return NotFound(new ErrorResponse("User not found"));
                }
            }
            catch (core.exceptions.businesslogic.services.ServiceUnavailableException ex)
            {
                _logger.LogWarning(ex, "Bonus Service unavailable, returning /me without privilege");
                // privilege stays null, continue with 200
            }

            // Get all tickets for this user using filter
            var userTickets = await _ticketService.GetAllAsync(new core.filters.TicketFilter { Username = username });

            // Flight is non-critical for /me as well
            Dictionary<string, core.domain.Flight> flightMap;
            try
            {
                var flights = await _flightGateway.GetAllAsync();
                flightMap = flights.ToDictionary(f => f.FlightNumber);
            }
            catch (core.exceptions.businesslogic.services.ServiceUnavailableException ex)
            {
                _logger.LogWarning(ex, "Flight Service unavailable (Circuit Breaker Open), returning /me without flight details");
                flightMap = new Dictionary<string, core.domain.Flight>();
            }
            catch (core.exceptions.dataaccess.gateways.GatewayCommunicationException ex)
            {
                _logger.LogWarning(ex, "Flight Service communication failed, returning /me without flight details");
                flightMap = new Dictionary<string, core.domain.Flight>();
            }

            // Use converter to build DTO with flight details
            var userInfo = UserHttpConverter.ToDTO(username, privilege, userTickets, flightMap);

            _logger.LogInformation("User information retrieved successfully: {Username}, {TicketCount} tickets", username, userInfo.Tickets.Count);
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user information for: {Username}", username);
            throw new UserInternalServerException(ex);
        }
    }
}
