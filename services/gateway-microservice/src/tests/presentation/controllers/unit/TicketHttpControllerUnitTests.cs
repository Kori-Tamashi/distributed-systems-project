using core.interfaces.businesslogic.services;
using core.filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using presentation.controllers.http;
using presentation.converters.http;
using presentation.dto.http.Ticket;
using presentation.exceptions.http;
using presentation.exceptions.http.Ticket;
using presentation.dto.http;
using tests.fixtures.builders;
using tests.config.attributes;
using tests.fixtures.mothers;

using ServiceTicketNotFoundException = core.exceptions.businesslogic.services.TicketNotFoundException;
using ServiceTicketValidationException = core.exceptions.businesslogic.services.TicketValidationException;
using HttpTicketNotFoundException = presentation.exceptions.http.Ticket.TicketNotFoundException;
using HttpTicketValidationException = presentation.exceptions.http.Ticket.TicketValidationException;
using TicketInternalServerException = presentation.exceptions.http.Ticket.TicketInternalServerException;

namespace tests.presentation.controllers.unit;

/// <summary>
/// Unit tests for TicketHttpController
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetTicketById(int ticketId):
/// - EP1: Valid ID, Ticket exists (200 OK)
/// - EP2: Valid ID, Ticket not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For GetAllTickets():
/// - EP1: Returns all tickets (200 OK)
/// - EP2: Returns empty list (200 OK)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For CreateTicket(CreateTicketDto):
/// - EP1: Valid ticket, creation successful (201 Created)
/// - EP2: Invalid ticket (400 Bad Request)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For UpdateTicket(int ticketId, UpdateTicketDto):
/// - EP1: Valid ticket, Ticket exists (200 OK)
/// - EP2: Ticket not found (404 Not Found)
/// - EP3: Invalid ticket (400 Bad Request)
/// - EP4: ID mismatch (400 Bad Request)
/// - EP5: Service throws exception (500 Internal Server Error)
/// 
/// For DeleteTicket(int ticketId):
/// - EP1: Valid ID, Ticket exists (200 OK)
/// - EP2: Valid ID, Ticket not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// Total: 17 unit tests (all should pass)
/// </summary>
public class TicketHttpControllerUnitTests
{
    private readonly Mock<ITicketService> _mockService;
    private readonly Mock<ILogger<TicketHttpController>> _mockLogger;
    private readonly TicketHttpController _controller;

    public TicketHttpControllerUnitTests()
    {
        // Arrange - Setup mocks
        _mockService = new Mock<ITicketService>();
        _mockLogger = new Mock<ILogger<TicketHttpController>>();
        
        _controller = new TicketHttpController(_mockService.Object, _mockLogger.Object);
        
        // Setup URL helper for Location header
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var routeData = new RouteData();
        routeData.Values.Add("area", string.Empty);
        routeData.Values.Add("controller", "Ticket");
        
        var urlHelper = new UrlHelper(new ActionContext(httpContext, routeData, new ActionDescriptor()));
        _controller.Url = urlHelper;
    }

    #region GetTicketById Tests

    /// <summary>
    /// EP1: Valid ID, Ticket exists - should return 200 OK with Ticket
    /// </summary>
    [Unit]
    public async Task GetTicketById_ValidId_TicketExists_ShouldReturnOk()
    {
        // Arrange
        var ticket = new TicketBuilder().WithUsername("test_user").Build();
        _mockService.Setup(s => s.GetByIdByUserAsync(ticket.TicketUid, It.IsAny<string>())).ReturnsAsync(ticket);
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "test_user";

        // Act
        var result = await _controller.GetTicketById(ticket.TicketUid);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TicketDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<TicketDTO>(okResult.Value);
        
        Assert.Equal(ticket.TicketUid, dto.TicketUid);
        _mockService.Verify(s => s.GetByIdByUserAsync(ticket.TicketUid, It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Ticket not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task GetTicketById_TicketNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdByUserAsync(ticketId, It.IsAny<string>()))
            .ThrowsAsync(new ServiceTicketNotFoundException(0));
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "test_user";

        // Act
        var result = await _controller.GetTicketById(ticketId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TicketDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        
        Assert.NotNull(dto);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetTicketById_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdByUserAsync(ticketId, It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "test_user";

        // Act & Assert
        await Assert.ThrowsAsync<TicketInternalServerException>(() => _controller.GetTicketById(ticketId));
    }

    #endregion

    #region GetMyTickets Tests

    /// <summary>
    /// EP1: Returns all tickets - should return 200 OK with list
    /// </summary>
    [Unit]
    public async Task GetMyTickets_ReturnsAllTickets_ShouldReturnOk()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(5);
        _mockService.Setup(s => s.GetAllAsync(It.Is<TicketFilter>(f => f.Username == "test_user"))).ReturnsAsync(tickets);
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "test_user";

        // Act
        var result = await _controller.GetMyTickets();

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<TicketDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<TicketDTO>>(okResult.Value);
        
        Assert.Equal(5, dtos.Count);
    }

    /// <summary>
    /// EP2: Returns empty list - should return 200 OK with empty list
    /// </summary>
    [Unit]
    public async Task GetMyTickets_EmptyList_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var tickets = new List<core.domain.Ticket>();
        _mockService.Setup(s => s.GetAllAsync(It.Is<TicketFilter>(f => f.Username == "test_user"))).ReturnsAsync(tickets);
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "test_user";

        // Act
        var result = await _controller.GetMyTickets();

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<TicketDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<TicketDTO>>(okResult.Value);
        
        Assert.Empty(dtos);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetMyTickets_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(It.IsAny<TicketFilter>()))
            .ThrowsAsync(new Exception("Database error"));
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "test_user";

        // Act & Assert
        await Assert.ThrowsAsync<TicketInternalServerException>(() => _controller.GetMyTickets());
    }

    #endregion

    #region BuyTicket Tests

    /// <summary>
    /// EP1: Valid ticket purchase - should return 200 OK with BuyTicketResponse
    /// </summary>
    [Unit]
    public async Task BuyTicket_ValidTicket_ShouldReturnCreated()
    {
        // Arrange
        var buyRequest = new BuyTicketRequest
        {
            FlightNumber = "AFL031",
            Price = 15000,
            PaidFromBalance = true
        };
        var buyResponse = (Guid.NewGuid(), 0, 15000);
        _mockService.Setup(s => s.BuyTicketAsync("john_doe", "AFL031", 15000, true))
            .ReturnsAsync(buyResponse);
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "john_doe";

        // Act
        var result = await _controller.BuyTicket(buyRequest);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BuyTicketResponse>>(result);
        var createdResult = Assert.IsType<CreatedResult>(actionResult.Result);
        var dto = Assert.IsType<BuyTicketResponse>(createdResult.Value);
        
        Assert.NotNull(dto.TicketUid);
        _mockService.Verify(s => s.BuyTicketAsync("john_doe", "AFL031", 15000, true), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid ticket - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task BuyTicket_InvalidTicket_ShouldReturnBadRequest()
    {
        // Arrange
        var buyRequest = new BuyTicketRequest { FlightNumber = "", Price = 0 };
        _mockService.Setup(s => s.BuyTicketAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ThrowsAsync(new ServiceTicketValidationException("Invalid ticket"));
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "john_doe";

        // Act
        var result = await _controller.BuyTicket(buyRequest);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BuyTicketResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<ValidationErrorResponse>(badRequestResult.Value);
        
        Assert.NotNull(dto);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task BuyTicket_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var buyRequest = new BuyTicketRequest { FlightNumber = "AFL031", Price = 15000 };
        _mockService.Setup(s => s.BuyTicketAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Database error"));
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "john_doe";

        // Act & Assert
        await Assert.ThrowsAsync<TicketInternalServerException>(() => _controller.BuyTicket(buyRequest));
    }

    #endregion

    #region ReturnTicket Tests

    /// <summary>
    /// EP1: Valid ID, Ticket exists - should return 204 No Content
    /// </summary>
    [Unit]
    public async Task ReturnTicket_ValidId_TicketExists_ShouldReturnNoContent()
    {
        // Arrange
        var ticketUid = Guid.NewGuid();
        _mockService.Setup(s => s.ReturnTicketAsync(ticketUid, It.IsAny<string>()))
            .ReturnsAsync(true);
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "test_user";

        // Act
        var result = await _controller.ReturnTicket(ticketUid);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        
        _mockService.Verify(s => s.ReturnTicketAsync(ticketUid, It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Ticket not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task ReturnTicket_TicketNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var ticketUid = Guid.NewGuid();
        _mockService.Setup(s => s.ReturnTicketAsync(ticketUid, It.IsAny<string>()))
            .ThrowsAsync(new ServiceTicketNotFoundException(0));
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "test_user";

        // Act
        var result = await _controller.ReturnTicket(ticketUid);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var dto = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        
        Assert.NotNull(dto);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task ReturnTicket_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var ticketUid = Guid.NewGuid();
        _mockService.Setup(s => s.ReturnTicketAsync(ticketUid, It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));
        
        _controller.ControllerContext.HttpContext.Request.Headers["X-User-Name"] = "test_user";

        // Act & Assert
        await Assert.ThrowsAsync<TicketInternalServerException>(() => _controller.ReturnTicket(ticketUid));
    }

    #endregion
}
