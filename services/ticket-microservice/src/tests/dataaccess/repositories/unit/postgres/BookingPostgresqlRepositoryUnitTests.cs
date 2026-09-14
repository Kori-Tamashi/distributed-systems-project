using System.Linq.Expressions;
using core.domain;
using core.enums;
using core.exceptions.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using dataaccess.repositories.postgres;
using Microsoft.EntityFrameworkCore;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

using BookingDomain = core.domain.Booking;
using BookingPostgresqlModel = dataaccess.models.postgres.BookingPostgresqlModel;

namespace tests.dataaccess.repositories.unit.postgres;

/// <summary>
/// Unit tests for BookingPostgresqlRepository
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// </summary>
public class BookingPostgresqlRepositoryUnitTests
{
    private readonly Mock<TicketsDatabaseContext> _mockContext;
    private readonly Mock<DbSet<BookingPostgresqlModel>> _mockDbSet;
    private readonly BookingPostgresqlRepository _repository;

    public BookingPostgresqlRepositoryUnitTests()
    {
        // Arrange - Setup mock context
        _mockContext = new Mock<TicketsDatabaseContext>();
        _mockDbSet = new Mock<DbSet<BookingPostgresqlModel>>();
        
        // Setup DbSet to behave like IQueryable
        var data = new List<BookingPostgresqlModel>().AsQueryable();
        _mockDbSet.As<IQueryable<BookingPostgresqlModel>>()
            .Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<BookingPostgresqlModel>>()
            .Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<BookingPostgresqlModel>>()
            .Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<BookingPostgresqlModel>>()
            .Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        
        _mockContext.Setup(c => c.Bookings).Returns(_mockDbSet.Object);
        _repository = new BookingPostgresqlRepository(_mockContext.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_BookingExists_ShouldReturnBooking()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        var model = new BookingPostgresqlModel
        {
            Id = booking.Id,
            BookingUid = booking.BookingUid,
            BookingReference = booking.BookingReference,
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            CustomerPhone = booking.CustomerPhone,
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
            Status = (int)booking.Status,
            PaymentMethod = (int)booking.PaymentMethod,
            PaymentTransactionId = booking.PaymentTransactionId
        };

        _mockDbSet.Setup(m => m.FindAsync(booking.Id)).ReturnsAsync(model);

        // Act
        var result = await _repository.GetByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(booking.CustomerName, result.CustomerName);
        _mockDbSet.Verify(m => m.FindAsync(booking.Id), Times.Once);
    }

    [Fact]
    [Unit]
    public async Task GetByIdAsync_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var bookingId = 999;
        _mockDbSet.Setup(m => m.FindAsync(bookingId)).ReturnsAsync((BookingPostgresqlModel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _repository.GetByIdAsync(bookingId)
        );
        Assert.Equal(bookingId, exception.BookingId);
    }

    [Fact]
    [Unit]
    public async Task GetByIdAsync_DatabaseError_ShouldThrowBookingDatabaseException()
    {
        // Arrange
        var bookingId = 1;
        _mockDbSet.Setup(m => m.FindAsync(bookingId))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingDatabaseException>(
            () => _repository.GetByIdAsync(bookingId)
        );
        Assert.Contains("Failed to get Booking by id", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    [Unit]
    public async Task CreateAsync_DatabaseError_ShouldThrowBookingDatabaseException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingDatabaseException>(
            () => _repository.CreateAsync(booking)
        );
        Assert.Contains("Failed to create Booking", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    [Unit]
    public async Task UpdateAsync_Success_ShouldReturnUpdatedBooking()
    {
        // Arrange
        var existingModel = new BookingPostgresqlModel
        {
            Id = 1,
            BookingUid = Guid.NewGuid(),
            BookingReference = "OLD123",
            CustomerName = "OLD NAME",
            CustomerEmail = "old@example.com",
            CustomerPhone = "+70000000000",
            TotalPrice = 10000,
            BookingDate = DateTime.Now,
            Status = (int)BookingStatus.Confirmed,
            PaymentMethod = (int)PaymentMethod.Cash,
            PaymentTransactionId = null
        };

        var updatedBooking = BookingMother.CreateValidBooking();
        updatedBooking.Id = 1;

        _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.UpdateAsync(updatedBooking);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedBooking.CustomerName, result.CustomerName);
        Assert.Equal(updatedBooking.TotalPrice, result.TotalPrice);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Unit]
    public async Task UpdateAsync_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        _mockDbSet.Setup(m => m.FindAsync(booking.Id)).ReturnsAsync((BookingPostgresqlModel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _repository.UpdateAsync(booking)
        );
        Assert.Equal(booking.Id, exception.BookingId);
    }

    [Fact]
    [Unit]
    public async Task UpdateAsync_DatabaseError_ShouldThrowBookingDatabaseException()
    {
        // Arrange
        var existingModel = new BookingPostgresqlModel { Id = 1, CustomerName = "OLD NAME" };
        var updatedBooking = BookingMother.CreateValidBooking();
        updatedBooking.Id = 1;

        _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingDatabaseException>(
            () => _repository.UpdateAsync(updatedBooking)
        );
        Assert.Contains("Failed to update Booking", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    [Unit]
    public async Task DeleteAsync_Success_ShouldReturnTrue()
    {
        // Arrange
        var bookingId = 1;
        var existingModel = new BookingPostgresqlModel { Id = bookingId, CustomerName = "John Doe" };

        _mockDbSet.Setup(m => m.FindAsync(bookingId)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(bookingId);

        // Assert
        Assert.True(result);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Unit]
    public async Task DeleteAsync_BookingNotFound_ShouldReturnFalse()
    {
        // Arrange
        var bookingId = 999;
        _mockDbSet.Setup(m => m.FindAsync(bookingId)).ReturnsAsync((BookingPostgresqlModel?)null);

        // Act
        var result = await _repository.DeleteAsync(bookingId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    [Unit]
    public async Task DeleteAsync_DatabaseError_ShouldThrowBookingDatabaseException()
    {
        // Arrange
        var bookingId = 1;
        var existingModel = new BookingPostgresqlModel { Id = bookingId, CustomerName = "John Doe" };

        _mockDbSet.Setup(m => m.FindAsync(bookingId)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingDatabaseException>(
            () => _repository.DeleteAsync(bookingId)
        );
        Assert.Contains("Failed to delete Booking", exception.Message);
    }

    #endregion
}
