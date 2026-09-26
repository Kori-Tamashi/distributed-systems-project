using core.interfaces.businesslogic.services;
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
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class UserHttpController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IPrivilegeService _privilegeService;
    private readonly ILogger<UserHttpController> _logger;

    /// <summary>
    /// Initializes a new instance of the UserHttpController
    /// </summary>
    /// <param name="ticketService">The Ticket business logic service</param>
    /// <param name="privilegeService">The Privilege business logic service</param>
    /// <param name="logger">The logger for the controller</param>
    public UserHttpController(
        ITicketService ticketService,
        IPrivilegeService privilegeService,
        ILogger<UserHttpController> logger)
    {
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        _privilegeService = privilegeService ?? throw new ArgumentNullException(nameof(privilegeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets complete user information including tickets and privilege status
    /// Aggregates data from Ticket and Privilege services
    /// </summary>
    /// <param name="username">Username from X-User-Name header</param>
    /// <returns>Complete user information with HTTP 200 OK</returns>
    /// <response code="200">Returns user information</response>
    /// <response code="400">Username is missing or invalid</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserInfoDTO>> GetUserInfo([FromHeader(Name = "X-User-Name")] string? username)
    {
        try
        {
            // Validate username
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Username header is missing or empty");
                return BadRequest(new ErrorResponse("Username is required"));
            }

            _logger.LogDebug("Getting user information for: {Username}", username);

            // Get privilege information by username using filter
            var userPrivilege = await _privilegeService.GetAllAsync(new core.filters.PrivilegeFilter { Username = username });
            var privilege = userPrivilege.FirstOrDefault();

            if (privilege == null)
            {
                _logger.LogWarning("User not found: {Username}", username);
                return NotFound(new ErrorResponse("User not found"));
            }

            // Get all tickets for this user using filter
            var userTickets = await _ticketService.GetAllAsync(new core.filters.TicketFilter { Username = username });

            // Use converter to build DTO
            var userInfo = UserHttpConverter.ToDTO(username, privilege, userTickets);

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
