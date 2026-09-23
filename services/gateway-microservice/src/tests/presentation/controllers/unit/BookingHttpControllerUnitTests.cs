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
using presentation.dto.http.Booking;
using presentation.exceptions.http;
using presentation.exceptions.http.Booking;
using tests.config.attributes;
using tests.fixtures.mothers;

using ServiceBookingNotFoundException = core.exceptions.businesslogic.services.BookingNotFoundException;
using ServiceBookingValidationException = core.exceptions.businesslogic.services.BookingValidationException;
using HttpBookingNotFoundException = presentation.exceptions.http.Booking.BookingNotFoundException;
using HttpBookingValidationException = presentation.exceptions.http.Booking.BookingValidationException;
using BookingInternalServerException = presentation.exceptions.http.Booking.BookingInternalServerException;

namespace tests.presentation.controllers.unit;

/// <summary>
/// Unit tests for BookingHttpController
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetBookingById(int bookingId):
/// - EP1: Valid ID, Booking exists (200 OK)
/// - EP2: Valid ID, Booking not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For GetAllBookings():
/// - EP1: Returns all bookings (200 OK)
/// - EP2: Returns empty list (200 OK)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For CreateBooking(CreateBookingDto):
/// - EP1: Valid booking, creation successful (201 Created)
/// - EP2: Invalid booking (400 Bad Request)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For UpdateBooking(int bookingId, UpdateBookingDto):
/// - EP1: Valid booking, Booking exists (200 OK)
/// - EP2: Booking not found (404 Not Found)
/// - EP3: Invalid booking (400 Bad Request)
/// - EP4: ID mismatch (400 Bad Request)
/// - EP5: Service throws exception (500 Internal Server Error)
/// 
/// For DeleteBooking(int bookingId):
/// - EP1: Valid ID, Booking exists (200 OK)
/// - EP2: Valid ID, Booking not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// Total: 17 unit tests (all should pass)
/// </summary>
public class BookingHttpControllerUnitTests
{
    private readonly Mock<IBookingService> _mockBookingService;
    private readonly Mock<ITicketService> _mockTicketService;
    private readonly Mock<ILogger<BookingHttpController>> _mockLogger;
    private readonly Mock<BookingSagaCoordinator> _mockSagaCoordinator;
    private readonly BookingHttpController _controller;

    public BookingHttpControllerUnitTests()
    {
        // Arrange - Setup mocks
        _mockBookingService = new Mock<IBookingService>();
        _mockTicketService = new Mock<ITicketService>();
        _mockLogger = new Mock<ILogger<BookingHttpController>>();
        _mockSagaCoordinator = new Mock<BookingSagaCoordinator>(
            Mock.Of<IBookingService>(),
            Mock.Of<ITicketService>(),
            Mock.Of<IPrivilegeService>(),
            Mock.Of<ILogger<BookingSagaCoordinator>>()
        );
        
        _controller = new BookingHttpController(
            _mockBookingService.Object,
            _mockTicketService.Object,
            _mockSagaCoordinator.Object,
            _mockLogger.Object
        );
        
        // Setup URL helper for Location header
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var routeData = new RouteData();
        routeData.Values.Add("area", string.Empty);
        routeData.Values.Add("controller", "Booking");
        
        var urlHelper = new UrlHelper(new ActionContext(httpContext, routeData, new ActionDescriptor()));
        _controller.Url = urlHelper;
    }

    #region GetBookingById Tests

    /// <summary>
    /// EP1: Valid ID, Booking exists - should return 200 OK with Booking
    /// </summary>
    [Unit]
    public async Task GetBookingById_ValidId_BookingExists_ShouldReturnOk()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingService.Setup(s => s.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        // Act
        var result = await _controller.GetBookingById(booking.Id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BookingDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<BookingDTO>(okResult.Value);
        
        Assert.Equal(booking.Id, dto.Id);
        _mockBookingService.Verify(s => s.GetByIdAsync(booking.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Booking not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task GetBookingById_BookingNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var bookingId = 999;
        _mockBookingService.Setup(s => s.GetByIdAsync(bookingId))
            .ThrowsAsync(new ServiceBookingNotFoundException(bookingId));

        // Act
        var result = await _controller.GetBookingById(bookingId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BookingDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpBookingNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(bookingId, dto.BookingId);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetBookingById_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingService.Setup(s => s.GetByIdAsync(bookingId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<BookingInternalServerException>(() => _controller.GetBookingById(bookingId));
    }

    #endregion

    #region GetAllBookings Tests

    /// <summary>
    /// EP1: Returns all bookings - should return 200 OK with list
    /// </summary>
    [Unit]
    public async Task GetAllBookings_ReturnsAllBookings_ShouldReturnOk()
    {
        // Arrange
        var bookings = BookingMother.CreateBookingList(5);
        _mockBookingService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(bookings);

        // Act
        var result = await _controller.GetAllBookings(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<BookingDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<BookingDTO>>(okResult.Value);
        
        Assert.Equal(5, dtos.Count);
    }

    /// <summary>
    /// EP2: Returns empty list - should return 200 OK with empty list
    /// </summary>
    [Unit]
    public async Task GetAllBookings_EmptyList_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var bookings = new List<core.domain.Booking>();
        _mockBookingService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(bookings);

        // Act
        var result = await _controller.GetAllBookings(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<BookingDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<BookingDTO>>(okResult.Value);
        
        Assert.Empty(dtos);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetAllBookings_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        _mockBookingService.Setup(s => s.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<BookingInternalServerException>(() => _controller.GetAllBookings(null, null));
    }

    #endregion

    #region CreateBooking Tests

    /// <summary>
    /// EP1: Valid booking, creation successful - should return 201 Created
    /// </summary>
    [Unit]
    public async Task CreateBooking_ValidBooking_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateBookingDTO
        {
            BookingReference = "REF123456",
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            CustomerPhone = "+1234567890",
            TotalPrice = 150000,
            BookingDate = DateTime.UtcNow.AddDays(-1)
        };
        var createdBooking = BookingMother.CreateValidBooking();
        createdBooking.Id = 1;
        _mockBookingService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Booking>()))
            .ReturnsAsync(createdBooking);

        // Act
        var result = await _controller.CreateBooking(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BookingDTO>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var dto = Assert.IsType<BookingDTO>(createdResult.Value);
        
        Assert.Equal(createdBooking.Id, dto.Id);
    }

    /// <summary>
    /// EP2: Invalid booking - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task CreateBooking_InvalidBooking_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateBookingDTO();
        _mockBookingService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Booking>()))
            .ThrowsAsync(new ServiceBookingValidationException("Invalid booking"));

        // Act
        var result = await _controller.CreateBooking(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BookingDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpBookingValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should throw SagaException (SAGA compensation)
    /// </summary>
    [Unit]
    public async Task CreateBooking_ServiceException_ShouldThrowSagaException()
    {
        // Arrange
        var createDto = new CreateBookingDTO();
        _mockBookingService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Booking>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<SagaException>(() => _controller.CreateBooking(createDto));
    }

    #endregion

    #region UpdateBooking Tests

    /// <summary>
    /// EP1: Valid booking, Booking exists - should return 200 OK
    /// </summary>
    [Unit]
    public async Task UpdateBooking_ValidBooking_BookingExists_ShouldReturnOk()
    {
        // Arrange
        var bookingId = 1;
        var updateDto = new UpdateBookingDTO
        {
            BookingReference = "REF789012",
            CustomerName = "Jane Doe",
            CustomerEmail = "jane@example.com",
            CustomerPhone = "+9876543210",
            TotalPrice = 200000,
            BookingDate = DateTime.UtcNow.AddDays(-2)
        };
        var updatedBooking = BookingMother.CreateValidBooking();
        updatedBooking.Id = bookingId;
        _mockBookingService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Booking>()))
            .ReturnsAsync(updatedBooking);

        // Act
        var result = await _controller.UpdateBooking(bookingId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BookingDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<BookingDTO>(okResult.Value);
        
        Assert.Equal(bookingId, dto.Id);
    }

    /// <summary>
    /// EP2: Booking not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task UpdateBooking_BookingNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var bookingId = 999;
        var updateDto = new UpdateBookingDTO();
        _mockBookingService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Booking>()))
            .ThrowsAsync(new ServiceBookingNotFoundException(bookingId));

        // Act
        var result = await _controller.UpdateBooking(bookingId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BookingDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpBookingNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(bookingId, dto.BookingId);
    }

    /// <summary>
    /// EP3: Invalid booking - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task UpdateBooking_InvalidBooking_ShouldReturnBadRequest()
    {
        // Arrange
        var bookingId = 1;
        var updateDto = new UpdateBookingDTO();
        _mockBookingService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Booking>()))
            .ThrowsAsync(new ServiceBookingValidationException("Invalid booking"));

        // Act
        var result = await _controller.UpdateBooking(bookingId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BookingDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpBookingValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP4: ID mismatch - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task UpdateBooking_IdMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var routeBookingId = 1;
        var updateDto = new UpdateBookingDTO
        {
            BookingReference = "REF789012",
            CustomerName = "Jane Doe",
            CustomerEmail = "jane@example.com",
            CustomerPhone = "+9876543210",
            TotalPrice = 200000,
            BookingDate = DateTime.UtcNow.AddDays(-2)
        };
        var existingBooking = BookingMother.CreateValidBooking();
        existingBooking.Id = 5; // Different ID from route
        _mockBookingService.Setup(s => s.GetByIdAsync(routeBookingId))
            .ReturnsAsync(existingBooking);

        // Act
        var result = await _controller.UpdateBooking(routeBookingId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<BookingDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpBookingValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP5: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task UpdateBooking_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var bookingId = 1;
        var updateDto = new UpdateBookingDTO();
        _mockBookingService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Booking>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<BookingInternalServerException>(() => _controller.UpdateBooking(bookingId, updateDto));
    }

    #endregion

    #region DeleteBooking Tests

    /// <summary>
    /// EP1: Valid ID, Booking exists - should return 204 No Content
    /// </summary>
    [Unit]
    public async Task DeleteBooking_ValidId_BookingExists_ShouldReturnNoContent()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingService.Setup(s => s.DeleteAsync(bookingId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteBooking(bookingId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        
        _mockBookingService.Verify(s => s.DeleteAsync(bookingId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Booking not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task DeleteBooking_BookingNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var bookingId = 999;
        _mockBookingService.Setup(s => s.DeleteAsync(bookingId))
            .ThrowsAsync(new ServiceBookingNotFoundException(bookingId));

        // Act
        var result = await _controller.DeleteBooking(bookingId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var dto = Assert.IsType<HttpBookingNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(bookingId, dto.BookingId);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task DeleteBooking_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingService.Setup(s => s.DeleteAsync(bookingId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<BookingInternalServerException>(() => _controller.DeleteBooking(bookingId));
    }

    #endregion
}
