using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Http;
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

namespace tests.presentation.controllers.unit;

/// <summary>
/// Unit tests for UserHttpController
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetUserInfo(string username):
/// - EP1: Valid username, user exists (200 OK)
/// - EP2: Username is missing (400 Bad Request)
/// - EP3: User not found (404 Not Found)
/// - EP4: Service throws exception (500 Internal Server Error)
/// 
/// Total: 5 unit tests (all should pass)
/// </summary>
public class UserHttpControllerUnitTests
{
    private readonly Mock<ITicketService> _mockTicketService;
    private readonly Mock<IPrivilegeService> _mockPrivilegeService;
    private readonly Mock<ILogger<UserHttpController>> _mockLogger;
    private readonly UserHttpController _controller;

    public UserHttpControllerUnitTests()
    {
        // Arrange - Setup mocks
        _mockTicketService = new Mock<ITicketService>();
        _mockPrivilegeService = new Mock<IPrivilegeService>();
        _mockLogger = new Mock<ILogger<UserHttpController>>();
        
        _controller = new UserHttpController(_mockTicketService.Object, _mockPrivilegeService.Object, _mockLogger.Object);
        
        // Setup HTTP context with headers
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-User-Name"] = "testuser";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var routeData = new RouteData();
        routeData.Values.Add("area", string.Empty);
        routeData.Values.Add("controller", "User");
        
        var urlHelper = new UrlHelper(new ActionContext(httpContext, routeData, new ActionDescriptor()));
        _controller.Url = urlHelper;
    }

    #region GetUserInfo Tests

    /// <summary>
    /// EP1: Valid username, user exists - should return 200 OK with user info
    /// </summary>
    [Unit]
    public async Task GetUserInfo_ValidUsername_UserExists_ShouldReturnOk()
    {
        // Arrange
        var username = "testuser";
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Username = username;
        var tickets = TicketMother.CreateTicketList(2);
        foreach (var ticket in tickets)
        {
            ticket.PassengerName = username;
        }
        
        _mockPrivilegeService.Setup(s => s.GetAllAsync(null))
            .ReturnsAsync(new List<core.domain.Privilege> { privilege });
        _mockTicketService.Setup(s => s.GetAllAsync(null))
            .ReturnsAsync(tickets);

        // Act
        var result = await _controller.GetUserInfo(username);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserInfoDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<UserInfoDTO>(okResult.Value);
        
        Assert.Equal(username, dto.Username);
        Assert.NotNull(dto.PrivilegeInfo);
        Assert.Equal(2, dto.Tickets.Count);
    }

    /// <summary>
    /// EP2: Username is missing - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task GetUserInfo_MissingUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.GetUserInfo(null!);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserInfoDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        
        Assert.NotNull(badRequestResult.Value);
    }

    /// <summary>
    /// EP3: User not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task GetUserInfo_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var username = "nonexistent";
        _mockPrivilegeService.Setup(s => s.GetAllAsync(null))
            .ReturnsAsync(new List<core.domain.Privilege>());

        // Act
        var result = await _controller.GetUserInfo(username);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserInfoDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        
        Assert.NotNull(notFoundResult.Value);
    }

    /// <summary>
    /// EP4: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetUserInfo_ServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        var username = "testuser";
        _mockPrivilegeService.Setup(s => s.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUserInfo(username);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserInfoDTO>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        
        Assert.Equal(500, objectResult.StatusCode);
    }

    /// <summary>
    /// EP5: User with no tickets - should return 200 OK with empty tickets list
    /// </summary>
    [Unit]
    public async Task GetUserInfo_UserHasNoTickets_ShouldReturnOkWithEmptyTickets()
    {
        // Arrange
        var username = "testuser";
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Username = username;
        var tickets = new List<core.domain.Ticket>();
        
        _mockPrivilegeService.Setup(s => s.GetAllAsync(null))
            .ReturnsAsync(new List<core.domain.Privilege> { privilege });
        _mockTicketService.Setup(s => s.GetAllAsync(null))
            .ReturnsAsync(tickets);

        // Act
        var result = await _controller.GetUserInfo(username);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserInfoDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<UserInfoDTO>(okResult.Value);
        
        Assert.Equal(username, dto.Username);
        Assert.NotNull(dto.PrivilegeInfo);
        Assert.Empty(dto.Tickets);
    }

    #endregion
}
