using core.domain;
using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using businesslogic.services;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

using RepositoryBookingAlreadyExistsException = core.exceptions.dataaccess.repositories.BookingAlreadyExistsException;
using RepositoryBookingNotFoundException = core.exceptions.dataaccess.repositories.BookingNotFoundException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for BookingService
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, Booking exists (normal case)
/// - EP2: Valid ID, Booking does not exist (BookingNotFoundException)
/// - EP3: Invalid ID (<= 0) (BookingValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetAllAsync(BookingFilter? filter):
/// - EP1: No filter, returns all bookings
/// - EP2: With filter, returns filtered bookings
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// For CreateAsync(Booking booking):
/// - EP1: Valid booking, creation successful (normal case)
/// - EP2: Invalid booking (BookingValidationException)
/// - EP3: Booking already exists (BookingBusinessRuleViolationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For UpdateAsync(Booking booking):
/// - EP1: Valid booking, Booking exists, update successful (normal case)
/// - EP2: Booking does not exist (BookingNotFoundException)
/// - EP3: Invalid booking (BookingValidationException)
/// - EP4: Invalid ID (BookingValidationException)
/// - EP5: Repository throws exception (BaseServiceException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, Booking exists, deletion successful (normal case)
/// - EP2: Valid ID, Booking does not exist (BookingNotFoundException)
/// - EP3: Invalid ID (<= 0) (BookingValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, Booking exists (returns true)
/// - EP2: Valid ID, Booking does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (BookingValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetCountAsync(BookingFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// Total: 23 unit tests (all should pass)
/// </summary>
public class BookingServiceUnitTests
{
    private readonly Mock<IBookingRepository> _mockBookingRepository;
    private readonly IBookingService _service;

    public BookingServiceUnitTests()
    {
        // Arrange - Setup mock repository
        _mockBookingRepository = new Mock<IBookingRepository>();
        _service = new BookingService(_mockBookingRepository.Object);
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid ID, Booking exists - should return Booking
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_BookingExists_ShouldReturnBooking()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingRepository.Setup(r => r.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        // Act
        var result = await _service.GetByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(booking.CustomerName, result.CustomerName);
        _mockBookingRepository.Verify(r => r.GetByIdAsync(booking.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Booking does not exist - should throw BookingNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var bookingId = 999;
        _mockBookingRepository.Setup(r => r.GetByIdAsync(bookingId)).ThrowsAsync(new RepositoryBookingNotFoundException(bookingId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _service.GetByIdAsync(bookingId)
        );
        Assert.Equal(bookingId, exception.BookingId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw BookingValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Unit]
    public async Task GetByIdAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingRepository.Setup(r => r.GetByIdAsync(bookingId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetByIdAsync(bookingId)
        );
        Assert.Contains($"Failed to get Booking with ID {bookingId}", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: No filter, returns all bookings - should return list of bookings
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllBookings()
    {
        // Arrange
        var bookings = BookingMother.CreateBookingList(5);
        _mockBookingRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(bookings);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        _mockBookingRepository.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered bookings - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new core.filters.BookingFilter { CustomerEmail = "test@example.com" };
        var filteredBookings = new List<Booking> { BookingMother.CreateValidBooking() };
        _mockBookingRepository.Setup(r => r.GetAllAsync(filter)).ReturnsAsync(filteredBookings);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockBookingRepository.Verify(r => r.GetAllAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to get all Bookings", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid booking, creation successful - should return created booking
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_ValidBooking_ShouldCreateBooking()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingRepository.Setup(r => r.CreateAsync(booking)).ReturnsAsync(booking);

        // Act
        var result = await _service.CreateAsync(booking);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.CustomerName, result.CustomerName);
        _mockBookingRepository.Verify(r => r.CreateAsync(booking), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid booking (null) - should throw BookingValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_NullBooking_ShouldThrowBookingValidationException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.CreateAsync(null!)
        );
        Assert.Contains("Booking cannot be null", exception.Message);
    }

    /// <summary>
    /// EP3: Invalid booking (invalid email) - should throw BookingValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_InvalidEmail_ShouldThrowBookingValidationException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.CustomerEmail = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.CreateAsync(booking)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Booking already exists - should throw BookingBusinessRuleViolationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_BookingAlreadyExists_ShouldThrowBookingBusinessRuleViolationException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingRepository.Setup(r => r.CreateAsync(booking))
            .ThrowsAsync(new RepositoryBookingAlreadyExistsException(booking.Id));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingBusinessRuleViolationException>(
            () => _service.CreateAsync(booking)
        );
        Assert.Contains("Booking", exception.Message);
    }

    /// <summary>
    /// EP5: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingRepository.Setup(r => r.CreateAsync(booking))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.CreateAsync(booking)
        );
        Assert.Contains("Failed to create Booking", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid booking, Booking exists, update successful - should return updated booking
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_ValidBooking_BookingExists_ShouldUpdateBooking()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingRepository.Setup(r => r.ExistsAsync(booking.Id)).ReturnsAsync(true);
        _mockBookingRepository.Setup(r => r.UpdateAsync(booking)).ReturnsAsync(booking);

        // Act
        var result = await _service.UpdateAsync(booking);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        _mockBookingRepository.Verify(r => r.ExistsAsync(booking.Id), Times.Once);
        _mockBookingRepository.Verify(r => r.UpdateAsync(booking), Times.Once);
    }

    /// <summary>
    /// EP2: Booking does not exist - should throw BookingNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingRepository.Setup(r => r.ExistsAsync(booking.Id)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _service.UpdateAsync(booking)
        );
        Assert.Equal(booking.Id, exception.BookingId);
    }

    /// <summary>
    /// EP3: Invalid booking (invalid price) - should throw BookingValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_InvalidPrice_ShouldThrowBookingValidationException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.TotalPrice = -100;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.UpdateAsync(booking)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID (<= 0) - should throw BookingValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task UpdateAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.UpdateAsync(booking)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    /// <summary>
    /// EP5: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockBookingRepository.Setup(r => r.ExistsAsync(booking.Id)).ReturnsAsync(true);
        _mockBookingRepository.Setup(r => r.UpdateAsync(booking))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.UpdateAsync(booking)
        );
        Assert.Contains($"Failed to update Booking with ID {booking.Id}", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid ID, Booking exists, deletion successful - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_BookingExists_ShouldDeleteBooking()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingRepository.Setup(r => r.ExistsAsync(bookingId)).ReturnsAsync(true);
        _mockBookingRepository.Setup(r => r.DeleteAsync(bookingId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(bookingId);

        // Assert
        Assert.True(result);
        _mockBookingRepository.Verify(r => r.ExistsAsync(bookingId), Times.Once);
        _mockBookingRepository.Verify(r => r.DeleteAsync(bookingId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Booking does not exist - should throw BookingNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var bookingId = 999;
        _mockBookingRepository.Setup(r => r.ExistsAsync(bookingId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _service.DeleteAsync(bookingId)
        );
        Assert.Equal(bookingId, exception.BookingId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw BookingValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task DeleteAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingRepository.Setup(r => r.ExistsAsync(bookingId)).ReturnsAsync(true);
        _mockBookingRepository.Setup(r => r.DeleteAsync(bookingId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.DeleteAsync(bookingId)
        );
        Assert.Contains($"Failed to delete Booking with ID {bookingId}", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Valid ID, Booking exists - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_BookingExists_ShouldReturnTrue()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingRepository.Setup(r => r.ExistsAsync(bookingId)).ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(bookingId);

        // Assert
        Assert.True(result);
        _mockBookingRepository.Verify(r => r.ExistsAsync(bookingId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Booking does not exist - should return false
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_BookingNotFound_ShouldReturnFalse()
    {
        // Arrange
        var bookingId = 999;
        _mockBookingRepository.Setup(r => r.ExistsAsync(bookingId)).ReturnsAsync(false);

        // Act
        var result = await _service.ExistsAsync(bookingId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw BookingValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task ExistsAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var bookingId = 1;
        _mockBookingRepository.Setup(r => r.ExistsAsync(bookingId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.ExistsAsync(bookingId)
        );
        Assert.Contains($"Failed to check existence of Booking with ID {bookingId}", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: No filter, returns correct count - should return count from repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_NoFilter_ShouldReturnCorrectCount()
    {
        // Arrange
        var expectedCount = 42;
        _mockBookingRepository.Setup(r => r.GetCountAsync(null)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(expectedCount, result);
        _mockBookingRepository.Verify(r => r.GetCountAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered count - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new core.filters.BookingFilter { MinTotalPrice = 1000, MaxTotalPrice = 5000 };
        var expectedCount = 5;
        _mockBookingRepository.Setup(r => r.GetCountAsync(filter)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync(filter);

        // Assert
        Assert.Equal(expectedCount, result);
        _mockBookingRepository.Verify(r => r.GetCountAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetCountAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to get Booking count", exception.Message);
    }

    #endregion
}
