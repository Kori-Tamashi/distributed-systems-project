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
using presentation.dto.http.Ticket;
using presentation.exceptions.http;
using presentation.exceptions.http.Ticket;
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
    private readonly Mock<BookingSagaCoordinator> _mockSagaCoordinator;
    private readonly TicketHttpController _controller;

    public TicketHttpControllerUnitTests()
    {
        // Arrange - Setup mocks
        _mockService = new Mock<ITicketService>();
        _mockLogger = new Mock<ILogger<TicketHttpController>>();
        _mockSagaCoordinator = new Mock<BookingSagaCoordinator>(
            Mock.Of<IBookingService>(),
            Mock.Of<ITicketService>(),
            Mock.Of<IPrivilegeService>(),
            Mock.Of<ILogger<BookingSagaCoordinator>>()
        );
        
        _controller = new TicketHttpController(_mockService.Object, _mockSagaCoordinator.Object, _mockLogger.Object);
        
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
        var ticket = TicketMother.CreateValidTicket();
        _mockService.Setup(s => s.GetByIdAsync(ticket.Id)).ReturnsAsync(ticket);

        // Act
        var result = await _controller.GetTicketById(ticket.Id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TicketDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<TicketDTO>(okResult.Value);
        
        Assert.Equal(ticket.Id, dto.Id);
        _mockService.Verify(s => s.GetByIdAsync(ticket.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Ticket not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task GetTicketById_TicketNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var ticketId = 999;
        _mockService.Setup(s => s.GetByIdAsync(ticketId))
            .ThrowsAsync(new ServiceTicketNotFoundException(ticketId));

        // Act
        var result = await _controller.GetTicketById(ticketId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TicketDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpTicketNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(ticketId, dto.TicketId);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetTicketById_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var ticketId = 1;
        _mockService.Setup(s => s.GetByIdAsync(ticketId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<TicketInternalServerException>(() => _controller.GetTicketById(ticketId));
    }

    #endregion

    #region GetAllTickets Tests

    /// <summary>
    /// EP1: Returns all tickets - should return 200 OK with list
    /// </summary>
    [Unit]
    public async Task GetAllTickets_ReturnsAllTickets_ShouldReturnOk()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(5);
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(tickets);

        // Act
        var result = await _controller.GetAllTickets(null, null);

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
    public async Task GetAllTickets_EmptyList_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var tickets = new List<core.domain.Ticket>();
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(tickets);

        // Act
        var result = await _controller.GetAllTickets(null, null);

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
    public async Task GetAllTickets_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<TicketInternalServerException>(() => _controller.GetAllTickets(null, null));
    }

    #endregion

    #region CreateTicket Tests

    /// <summary>
    /// EP1: Valid ticket, creation successful - should return 201 Created
    /// </summary>
    [Unit]
    public async Task CreateTicket_ValidTicket_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateTicketDTO
        {
            FlightId = 1,
            PassengerName = "John Doe",
            PassengerEmail = "john@example.com",
            PassengerPhone = "+1234567890",
            SeatNumber = "12A",
            Class = 0,
            Price = 50000,
            BookingDate = DateTime.UtcNow.AddDays(-1),
            Status = 0
        };
        var createdTicket = TicketMother.CreateValidTicket();
        createdTicket.Id = 1;
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Ticket>()))
            .ReturnsAsync(createdTicket);

        // Act
        var result = await _controller.CreateTicket(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TicketDTO>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var dto = Assert.IsType<TicketDTO>(createdResult.Value);
        
        Assert.Equal(createdTicket.Id, dto.Id);
    }

    /// <summary>
    /// EP2: Invalid ticket - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task CreateTicket_InvalidTicket_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateTicketDTO();
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Ticket>()))
            .ThrowsAsync(new ServiceTicketValidationException("Invalid ticket"));

        // Act
        var result = await _controller.CreateTicket(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TicketDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpTicketValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task CreateTicket_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var createDto = new CreateTicketDTO();
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Ticket>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<TicketInternalServerException>(() => _controller.CreateTicket(createDto));
    }

    #endregion

    #region UpdateTicket Tests

    /// <summary>
    /// EP1: Valid ticket, Ticket exists - should return 200 OK
    /// </summary>
    [Unit]
    public async Task UpdateTicket_ValidTicket_TicketExists_ShouldReturnOk()
    {
        // Arrange
        var ticketId = 1;
        var updateDto = new UpdateTicketDTO
        {
            PassengerName = "Jane Doe",
            PassengerEmail = "jane@example.com",
            PassengerPhone = "+9876543210",
            SeatNumber = "15B",
            Class = 1,
            Price = 60000,
            BookingDate = DateTime.UtcNow.AddDays(-2),
            Status = 1
        };
        var updatedTicket = TicketMother.CreateValidTicket();
        updatedTicket.Id = ticketId;
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Ticket>()))
            .ReturnsAsync(updatedTicket);

        // Act
        var result = await _controller.UpdateTicket(ticketId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TicketDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<TicketDTO>(okResult.Value);
        
        Assert.Equal(ticketId, dto.Id);
    }

    /// <summary>
    /// EP2: Ticket not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task UpdateTicket_TicketNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var ticketId = 999;
        var updateDto = new UpdateTicketDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Ticket>()))
            .ThrowsAsync(new ServiceTicketNotFoundException(ticketId));

        // Act
        var result = await _controller.UpdateTicket(ticketId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TicketDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpTicketNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(ticketId, dto.TicketId);
    }

    /// <summary>
    /// EP3: Invalid ticket - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task UpdateTicket_InvalidTicket_ShouldReturnBadRequest()
    {
        // Arrange
        var ticketId = 1;
        var updateDto = new UpdateTicketDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Ticket>()))
            .ThrowsAsync(new ServiceTicketValidationException("Invalid ticket"));

        // Act
        var result = await _controller.UpdateTicket(ticketId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TicketDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpTicketValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP4: ID mismatch - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task UpdateTicket_IdMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var routeTicketId = 1;
        var updateDto = new UpdateTicketDTO
        {
            PassengerName = "Jane Doe",
            PassengerEmail = "jane@example.com",
            PassengerPhone = "+9876543210",
            SeatNumber = "15B",
            Class = 1,
            Price = 60000,
            BookingDate = DateTime.UtcNow.AddDays(-2),
            Status = 1
        };
        var existingTicket = TicketMother.CreateValidTicket();
        existingTicket.Id = 5; // Different ID from route
        _mockService.Setup(s => s.GetByIdAsync(routeTicketId))
            .ReturnsAsync(existingTicket);

        // Act
        var result = await _controller.UpdateTicket(routeTicketId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TicketDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpTicketValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP5: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task UpdateTicket_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var ticketId = 1;
        var updateDto = new UpdateTicketDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Ticket>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<TicketInternalServerException>(() => _controller.UpdateTicket(ticketId, updateDto));
    }

    #endregion

    #region DeleteTicket Tests

    /// <summary>
    /// EP1: Valid ID, Ticket exists - should return 204 No Content
    /// </summary>
    [Unit]
    public async Task DeleteTicket_ValidId_TicketExists_ShouldReturnNoContent()
    {
        // Arrange
        var ticketId = 1;
        _mockService.Setup(s => s.DeleteAsync(ticketId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteTicket(ticketId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        
        _mockService.Verify(s => s.DeleteAsync(ticketId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Ticket not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task DeleteTicket_TicketNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var ticketId = 999;
        _mockService.Setup(s => s.DeleteAsync(ticketId))
            .ThrowsAsync(new ServiceTicketNotFoundException(ticketId));

        // Act
        var result = await _controller.DeleteTicket(ticketId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var dto = Assert.IsType<HttpTicketNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(ticketId, dto.TicketId);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task DeleteTicket_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var ticketId = 1;
        _mockService.Setup(s => s.DeleteAsync(ticketId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<TicketInternalServerException>(() => _controller.DeleteTicket(ticketId));
    }

    #endregion
}
