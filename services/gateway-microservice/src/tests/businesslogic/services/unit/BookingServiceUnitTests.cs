using core.domain;
using Microsoft.Extensions.Logging;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using businesslogic.services;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

using GatewayBookingNotFoundException = core.exceptions.dataaccess.gateways.BookingGatewayEntityNotFoundException;
using GatewayBookingCommunicationException = core.exceptions.dataaccess.gateways.BookingGatewayCommunicationException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for BookingService
/// Using London-style TDD with Mocks (Moq) for HTTP Gateways
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, Booking exists (normal case)
/// - EP2: Valid ID, Booking does not exist (GatewayBookingNotFoundException)
/// - EP3: Invalid ID (<= 0) (BookingValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetAllAsync(BookingFilter? filter):
/// - EP1: No filter, returns all bookings
/// - EP2: With filter, returns filtered bookings
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For CreateAsync(Booking booking):
/// - EP1: Valid booking, creation successful (normal case)
/// - EP2: Invalid booking (BookingValidationException)
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For UpdateAsync(Booking booking):
/// - EP1: Valid booking, Booking exists, update successful (normal case)
/// - EP2: Booking does not exist (GatewayBookingNotFoundException)
/// - EP3: Invalid booking (BookingValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, Booking exists, deletion successful (normal case)
/// - EP2: Valid ID, Booking does not exist (GatewayBookingNotFoundException)
/// - EP3: Invalid ID (<= 0) (BookingValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, Booking exists (returns true)
/// - EP2: Valid ID, Booking does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (BookingValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetCountAsync(BookingFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Gateway communication error (ValidationException)
/// 
/// Total: 21 unit tests (all should pass)
/// </summary>
public class BookingServiceUnitTests
{
    private readonly Mock<IBookingGateway> _mockBookingGateway;
    private readonly IBookingService _service;

    public BookingServiceUnitTests()
    {
        // Arrange - Setup mock gateway
        _mockBookingGateway = new Mock<IBookingGateway>();
        _service = new BookingService(_mockBookingGateway.Object, Mock.Of<ILogger<BookingService>>());
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid ID, Booking exists - should return Booking
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ValidId_BookingExists_ShouldReturnBooking()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingGateway.Setup(g => g.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        // Act
        var result = await _service.GetByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(booking.CustomerName, result.CustomerName);
        _mockBookingGateway.Verify(g => g.GetByIdAsync(booking.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Booking does not exist - should throw BookingNotFoundException
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ValidId_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var bookingId = 999;
        _mockBookingGateway.Setup(g => g.DeleteAsync(bookingId)).ThrowsAsync(new GatewayBookingNotFoundException($"Booking with id {bookingId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _service.GetByIdAsync(bookingId)
        );
        Assert.Equal(bookingId, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw BookingValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetByIdAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingGateway.Setup(g => g.GetByIdAsync(bookingId))
            .ThrowsAsync(new GatewayBookingCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetByIdAsync(bookingId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: No filter, returns all bookings
    /// </summary>
    [Fact]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllBookings()
    {
        // Arrange
        var bookings = BookingMother.CreateBookingList(5);
        _mockBookingGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        _mockBookingGateway.Verify(g => g.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered bookings
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredBookings()
    {
        // Arrange
        var bookings = BookingMother.CreateBookingList(3);
        var filter = new BookingFilter { CustomerName = "John" };
        _mockBookingGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(bookings);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        _mockBookingGateway.Verify(g => g.GetAllAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task GetAllAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        _mockBookingGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayBookingCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid booking, creation successful - should return created booking
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidBooking_ShouldReturnCreatedBooking()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingGateway.Setup(g => g.CreateAsync(It.IsAny<Booking>())).ReturnsAsync(booking);

        // Act
        var result = await _service.CreateAsync(booking);

        // Assert
        Assert.NotNull(result);
        _mockBookingGateway.Verify(g => g.CreateAsync(It.IsAny<Booking>()), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid booking (empty customer name) - should throw BookingValidationException
    /// </summary>
    [Fact]
    public async Task CreateAsync_InvalidBooking_ShouldThrowBookingValidationException()
    {
        // Arrange
        var booking = new Booking { CustomerName = "", CustomerEmail = "test@test.com", TotalPrice = 1000, BookingDate = DateTime.UtcNow.AddHours(-1) };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.CreateAsync(booking)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP3: Future booking date - should throw BookingValidationException
    /// </summary>
    [Fact]
    public async Task CreateAsync_FutureBookingDate_ShouldThrowBookingValidationException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.BookingDate = DateTime.UtcNow.AddDays(30);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.CreateAsync(booking)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task CreateAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingGateway.Setup(g => g.CreateAsync(It.IsAny<Booking>()))
            .ThrowsAsync(new GatewayBookingCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(booking)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid booking, Booking exists, update successful - should return updated booking
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ValidBooking_BookingExists_ShouldReturnUpdatedBooking()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingGateway.Setup(g => g.UpdateAsync(It.IsAny<Booking>())).ReturnsAsync(booking);

        // Act
        var result = await _service.UpdateAsync(booking);

        // Assert
        Assert.NotNull(result);
        _mockBookingGateway.Verify(g => g.UpdateAsync(It.IsAny<Booking>()), Times.Once);
    }

    /// <summary>
    /// EP2: Booking does not exist - should throw BookingNotFoundException
    /// </summary>
    [Fact]
    public async Task UpdateAsync_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingGateway.Setup(g => g.UpdateAsync(It.IsAny<Booking>())).ThrowsAsync(new GatewayBookingNotFoundException($"Booking with id {booking.Id} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _service.UpdateAsync(booking)
        );
        Assert.Equal(booking.Id, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid booking - should throw BookingValidationException
    /// </summary>
    [Fact]
    public async Task UpdateAsync_InvalidBooking_ShouldThrowBookingValidationException()
    {
        // Arrange
        var booking = new Booking { Id = 1, CustomerName = "", CustomerEmail = "test@test.com", TotalPrice = 1000, BookingDate = DateTime.UtcNow.AddHours(-1) };
        _mockBookingGateway.Setup(g => g.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.UpdateAsync(booking)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task UpdateAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingGateway.Setup(g => g.GetByIdAsync(booking.Id)).ReturnsAsync(booking);
        _mockBookingGateway.Setup(g => g.UpdateAsync(It.IsAny<Booking>()))
            .ThrowsAsync(new GatewayBookingCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.UpdateAsync(booking)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid ID, Booking exists, deletion successful - should return true
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ValidId_BookingExists_ShouldReturnTrue()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingGateway.Setup(g => g.GetByIdAsync(bookingId)).ReturnsAsync(BookingMother.CreateValidBooking());
        _mockBookingGateway.Setup(g => g.DeleteAsync(bookingId)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(bookingId);

        // Assert
        _mockBookingGateway.Verify(g => g.DeleteAsync(bookingId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Booking does not exist - should throw BookingNotFoundException
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ValidId_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var bookingId = 999;
        _mockBookingGateway.Setup(g => g.DeleteAsync(bookingId)).ThrowsAsync(new GatewayBookingNotFoundException($"Booking with id {bookingId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _service.DeleteAsync(bookingId)
        );
        Assert.Equal(bookingId, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw BookingValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task DeleteAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingGateway.Setup(g => g.GetByIdAsync(bookingId)).ReturnsAsync(BookingMother.CreateValidBooking());
        _mockBookingGateway.Setup(g => g.DeleteAsync(bookingId))
            .ThrowsAsync(new GatewayBookingCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.DeleteAsync(bookingId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Valid ID, Booking exists - should return true
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ValidId_BookingExists_ShouldReturnTrue()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingGateway.Setup(g => g.GetByIdAsync(bookingId)).ReturnsAsync(BookingMother.CreateValidBooking());

        // Act
        var result = await _service.ExistsAsync(bookingId);

        // Assert
    }

    /// <summary>
    /// EP2: Valid ID, Booking does not exist - should return false
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ValidId_BookingNotFound_ShouldReturnFalse()
    {
        // Arrange
        var bookingId = 999;
        _mockBookingGateway.Setup(g => g.GetByIdAsync(bookingId)).ReturnsAsync((Booking?)null);

        // Act
        var result = await _service.ExistsAsync(bookingId);

        // Assert
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw BookingValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExistsAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task ExistsAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingGateway.Setup(g => g.GetByIdAsync(bookingId))
            .ThrowsAsync(new GatewayBookingCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.ExistsAsync(bookingId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: No filter, returns correct count
    /// </summary>
    [Fact]
    public async Task GetCountAsync_NoFilter_ShouldReturnCorrectCount()
    {
        // Arrange
        var bookings = BookingMother.CreateBookingList(10);
        _mockBookingGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(10, result);
    }

    /// <summary>
    /// EP2: With filter, returns filtered count
    /// </summary>
    [Fact]
    public async Task GetCountAsync_WithFilter_ShouldReturnFilteredCount()
    {
        // Arrange
        var bookings = BookingMother.CreateBookingList(5);
        var filter = new BookingFilter { CustomerName = "John" };
        _mockBookingGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(bookings);

        // Act
        var result = await _service.GetCountAsync(filter);

        // Assert
        Assert.Equal(5, result);
    }

    /// <summary>
    /// EP3: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task GetCountAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        _mockBookingGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayBookingCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion
}
