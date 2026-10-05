using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using core.filters;
using Microsoft.AspNetCore.Http;
using presentation.exceptions.http.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using presentation.controllers.http;
using presentation.converters.http;
using presentation.dto.http.User;
using tests.config.attributes;
using tests.fixtures.mothers;
using System.Security.Claims;
using presentation.exceptions.http;

namespace tests.presentation.controllers.unit;

/// <summary>
/// Unit tests for UserHttpController
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetUserInfo():
/// - EP1: Valid username in JWT, user exists (200 OK)
/// - EP2: Username is missing in JWT (400 Bad Request)
/// - EP3: User not found (404 Not Found)
/// - EP4: Service throws exception (500 Internal Server Error)
/// 
/// Total: 5 unit tests (all should pass)
/// </summary>
public class UserHttpControllerUnitTests
{
    private readonly Mock<ITicketService> _mockTicketService;
    private readonly Mock<IPrivilegeService> _mockPrivilegeService;
    private readonly Mock<core.interfaces.dataaccess.gateways.IFlightGateway> _mockFlightGateway;
    private readonly Mock<ILogger<UserHttpController>> _mockLogger;
    private readonly UserHttpController _controller;

    public UserHttpControllerUnitTests()
    {
        // Arrange - Setup mocks
        _mockTicketService = new Mock<ITicketService>();
        _mockPrivilegeService = new Mock<IPrivilegeService>();
        _mockFlightGateway = new Mock<core.interfaces.dataaccess.gateways.IFlightGateway>();
        _mockLogger = new Mock<ILogger<UserHttpController>>();
        
        _controller = new UserHttpController(_mockTicketService.Object, _mockPrivilegeService.Object, _mockFlightGateway.Object, _mockLogger.Object);
        
        var routeData = new RouteData();
        routeData.Values.Add("area", string.Empty);
        routeData.Values.Add("controller", "User");
        
        var urlHelper = new UrlHelper(new ActionContext(new DefaultHttpContext(), routeData, new ActionDescriptor()));
        _controller.Url = urlHelper;
    }

    #region GetUserInfo Tests

    /// <summary>
    /// EP1: Valid username in JWT, user exists - should return 200 OK with user info
    /// </summary>
    [Unit]
    public async Task GetUserInfo_ValidUsername_UserExists_ShouldReturnOk()
    {
        // Arrange
        var username = "testuser";
        SetupJwtUser(username);
        
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Username = username;
        var tickets = TicketMother.CreateTicketList(2);
        foreach (var ticket in tickets)
        {
            ticket.Username = username;
        }
        
        _mockPrivilegeService.Setup(s => s.GetAllAsync(It.IsAny<PrivilegeFilter>()))
            .ReturnsAsync(new List<core.domain.Privilege> { privilege });
        _mockTicketService.Setup(s => s.GetAllAsync(It.IsAny<TicketFilter>()))
            .ReturnsAsync(tickets);
        _mockFlightGateway.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<core.domain.Flight>());

        // Act
        var result = await _controller.GetUserInfo();

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserInfoDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<UserInfoDTO>(okResult.Value);

        Assert.NotNull(dto);
        Assert.Equal(username, dto.Username);
        Assert.NotNull(dto.Tickets);
        Assert.NotNull(dto.PrivilegeInfo);
    }

    /// <summary>
    /// EP2: Username is missing in JWT - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task GetUserInfo_MissingUsername_ShouldReturnBadRequest()
    {
        // Arrange - empty claims, no username
        SetupJwtUser(null);

        // Act
        var result = await _controller.GetUserInfo();

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserInfoDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

        Assert.NotNull(badRequestResult.Value);
    }

    /// <summary>
    /// EP3: Valid username in JWT, user not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task GetUserInfo_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var username = "testuser";
        SetupJwtUser(username);
        
        _mockPrivilegeService.Setup(s => s.GetAllAsync(It.IsAny<PrivilegeFilter>()))
            .ReturnsAsync(new List<core.domain.Privilege>());
        _mockFlightGateway.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<core.domain.Flight>());

        // Act
        var result = await _controller.GetUserInfo();

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserInfoDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        
        Assert.NotNull(notFoundResult.Value);
    }

    /// <summary>
    /// EP4: Service throws exception - should propagate exception
    /// </summary>
    [Unit]
    public async Task GetUserInfo_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var username = "testuser";
        SetupJwtUser(username);
        
        _mockPrivilegeService.Setup(s => s.GetAllAsync(It.IsAny<PrivilegeFilter>()))
            .ThrowsAsync(new UserInternalServerException(new Exception("Service error")));

        // Act & Assert
        await Assert.ThrowsAsync<UserInternalServerException>(
            () => _controller.GetUserInfo());
    }

    /// <summary>
    /// EP1 variant: User has no tickets - should return 200 OK with empty tickets list
    /// </summary>
    [Unit]
    public async Task GetUserInfo_UserHasNoTickets_ShouldReturnOkWithEmptyTickets()
    {
        // Arrange
        var username = "testuser";
        SetupJwtUser(username);
        
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Username = username;
        
        _mockPrivilegeService.Setup(s => s.GetAllAsync(It.IsAny<PrivilegeFilter>()))
            .ReturnsAsync(new List<core.domain.Privilege> { privilege });
        _mockTicketService.Setup(s => s.GetAllAsync(It.IsAny<TicketFilter>()))
            .ReturnsAsync(new List<core.domain.Ticket>());
        _mockFlightGateway.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<core.domain.Flight>());

        // Act
        var result = await _controller.GetUserInfo();

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserInfoDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<UserInfoDTO>(okResult.Value);

        Assert.NotNull(dto);
        Assert.Equal(username, dto.Username);
        Assert.NotNull(dto.Tickets);
        Assert.Empty(dto.Tickets);
        Assert.NotNull(dto.PrivilegeInfo);
    }

    #endregion

    /// <summary>
    /// Helper: Setup JWT user with username in claims
    /// </summary>
    private void SetupJwtUser(string? username)
    {
        var identity = new ClaimsIdentity();
        if (!string.IsNullOrWhiteSpace(username))
        {
            identity.AddClaim(new Claim("preferred_username", username));
        }
        
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }
}
