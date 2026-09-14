using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using businesslogic.services;
using core.domain;
using core.enums;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.repositories.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.contexts.postgres;
using tests.fixtures.mothers;
using DotNetEnv;
using Xunit;

using ServiceBookingNotFoundException = core.exceptions.businesslogic.services.BookingNotFoundException;
using ServiceBookingValidationException = core.exceptions.businesslogic.services.BookingValidationException;
using BookingDomain = core.domain.Booking;

namespace tests.businesslogic.services.integration.postgres;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for BookingService
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_tickets).
/// BookingService is tested with real BookingPostgresqlRepository.
/// 
/// TEST CONTEXT:
/// - TestPostgresDatabaseContext is used for database operations
/// - Database is created once in constructor and cleaned after each test
/// - Each test method cleans up its own data in Dispose
/// - Tests share the same database but are isolated by cleanup
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetByIdAsync Tests:
///    EP1: Valid existing ID (normal case) - booking exists in database
///    EP2: Non-existing ID (error case) - BookingNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - BookingValidationException thrown
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single booking (normal case) - should return list with one booking
///    EP3: Multiple bookings (normal case) - should return all bookings
///    EP4: Filter by customer email (normal case) - should return matching bookings
///    EP5: Filter by status (normal case) - should return matching bookings
///    EP6: Filter with no matches (edge case) - should return empty list
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid booking with all fields (normal case) - should create successfully
///    EP2: Valid booking with minimal data (edge case) - should create successfully
///    EP3: Booking with empty customer name (validation case) - BookingValidationException thrown
///    EP4: Booking with empty email (validation case) - BookingValidationException thrown
///    EP5: Booking with future booking date (validation case) - BookingValidationException thrown
///    EP6: Booking with negative price (validation case) - BookingValidationException thrown
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Non-existing ID (error case) - BookingNotFoundException thrown
///    EP3: Invalid customer name (validation case) - BookingValidationException thrown
///    EP4: Invalid ID <= 0 (validation case) - BookingValidationException thrown
/// 
/// 5. DeleteAsync Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (error case) - BookingNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - BookingValidationException thrown
///    EP4: Delete and verify data removed (verification case) - GetById throws
/// 
/// 6. ExistsAsync Tests:
///    EP1: Existing ID (normal case) - should return true
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: ID after deletion (verification case) - should return false
///    EP4: Invalid ID <= 0 (validation case) - BookingValidationException thrown
/// 
/// 7. GetCountAsync Tests:
///    EP1: Empty database (edge case) - should return 0
///    EP2: Single booking (normal case) - should return 1
///    EP3: Multiple bookings (normal case) - should return correct count
/// 
/// AAA STRUCTURE:
/// All tests follow Arrange-Act-Assert pattern:
/// - Arrange: Setup test data and database state
/// - Act: Execute the method under test
/// - Assert: Verify the results
/// 
/// Total: 27 integration tests
/// </summary>
public class BookingServiceIntegrationTests : IDisposable
{
    private readonly TestPostgresDatabaseContext _testContext;
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingService _service;

    static BookingServiceIntegrationTests()
    {
        // Load .env file to get TEST_POSTGRESQL_* environment variables
        DotNetEnv.Env.Load();
    }

    public BookingServiceIntegrationTests()
    {
        // Create test database context - this uses TEST_* environment variables
        _testContext = new TestPostgresDatabaseContext();
        _testContext.EnsureDatabaseDeleted(); // Create tables automatically
        _testContext.EnsureDatabaseCreated();
        
        // Create repositories and service using the test database context
        _bookingRepository = new BookingPostgresqlRepository(_testContext);
        _service = new BookingService(_bookingRepository);
    }

    public void Dispose()
    {
        try
        {
            // Clean up all bookings
            var bookings = _testContext.Bookings.ToList();
            if (bookings.Any())
            {
                _testContext.Bookings.RemoveRange(bookings);
                _testContext.SaveChanges();
            }
        }
        catch
        {
            // Ignore errors during cleanup (database might not exist)
        }
        finally
        {
            _testContext.Dispose();
        }
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return booking
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_BookingExists_ShouldReturnBooking()
    {
        // Arrange
        var booking = new BookingBuilder().WithId(0).WithCustomerName("John Doe").WithCustomerEmail("john@example.com").Build();
        var created = await _service.CreateAsync(booking);

        // Act
        var result = await _service.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.CustomerName, result.CustomerName);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw BookingNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingNotFoundException>(
            () => _service.GetByIdAsync(nonExistentId)
        );
        Assert.Equal(nonExistentId, exception.BookingId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw BookingValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: Empty database - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// EP2: Single booking - should return list with one booking
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SingleBooking_ShouldReturnOneBooking()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;
        await _service.CreateAsync(booking);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    /// <summary>
    /// EP3: Multiple bookings - should return all bookings
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultipleBookings_ShouldReturnAllBookings()
    {
        // Arrange
        var bookings = BookingMother.CreateBookingList(5);
        foreach (var booking in bookings)
        {
            booking.Id = 0;
            await _service.CreateAsync(booking);
        }

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
    }

    /// <summary>
    /// EP4: Filter by customer email - should return matching bookings
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithEmailFilter_ShouldReturnMatchingBookings()
    {
        // Arrange
        var booking1 = new BookingBuilder().WithId(0).WithCustomerEmail("test@example.com").Build();
        var booking2 = new BookingBuilder().WithId(0).WithCustomerEmail("other@example.com").Build();
        await _service.CreateAsync(booking1);
        await _service.CreateAsync(booking2);

        // Act
        var result = await _service.GetAllAsync(new BookingFilter { CustomerEmail = "test@example.com" });

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("test@example.com", result[0].CustomerEmail);
    }

    /// <summary>
    /// EP5: Filter by status - should return matching bookings
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithStatusFilter_ShouldReturnMatchingBookings()
    {
        // Arrange
        var booking1 = new BookingBuilder().WithId(0).WithConfirmedStatus().Build();
        var booking2 = new BookingBuilder().WithId(0).WithCancelledStatus().Build();
        await _service.CreateAsync(booking1);
        await _service.CreateAsync(booking2);

        // Act
        var result = await _service.GetAllAsync(new BookingFilter { Status = (int)BookingStatus.Confirmed });

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal((int)BookingStatus.Confirmed, (int)result[0].Status);
    }

    /// <summary>
    /// EP6: Filter with no matches - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilterNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;
        await _service.CreateAsync(booking);

        // Act
        var result = await _service.GetAllAsync(new BookingFilter { CustomerEmail = "nonexistent@example.com" });

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid booking with all fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidBooking_ShouldCreateBooking()
    {
        // Arrange
        var booking = new BookingBuilder().WithId(0).WithCustomerName("John Doe").WithCustomerEmail("john@example.com").Build();

        // Act
        var result = await _service.CreateAsync(booking);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("John Doe", result.CustomerName);
    }

    /// <summary>
    /// EP2: Valid booking with minimal data - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_MinimalBooking_ShouldCreateBooking()
    {
        // Arrange
        var booking = BookingMother.CreateMinimalBooking();
        booking.Id = 0;

        // Act
        var result = await _service.CreateAsync(booking);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
    }

    /// <summary>
    /// EP3: Booking with empty customer name - should throw BookingValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_EmptyCustomerName_ShouldThrowBookingValidationException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;
        booking.CustomerName = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingValidationException>(
            () => _service.CreateAsync(booking)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Booking with empty email - should throw BookingValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_EmptyEmail_ShouldThrowBookingValidationException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;
        booking.CustomerEmail = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingValidationException>(
            () => _service.CreateAsync(booking)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP5: Booking with future booking date - should throw BookingValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_FutureBookingDate_ShouldThrowBookingValidationException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;
        booking.BookingDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingValidationException>(
            () => _service.CreateAsync(booking)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP6: Booking with negative price - should throw BookingValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_NegativePrice_ShouldThrowBookingValidationException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;
        booking.TotalPrice = -100;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingValidationException>(
            () => _service.CreateAsync(booking)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid update with all fields - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_ValidUpdate_ShouldUpdateBooking()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;
        var created = await _service.CreateAsync(booking);
        created.CustomerName = "Jane Doe";

        // Act
        var result = await _service.UpdateAsync(created);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane Doe", result.CustomerName);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw BookingNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingNotFoundException>(
            () => _service.UpdateAsync(booking)
        );
        Assert.Equal(booking.Id, exception.BookingId);
    }

    /// <summary>
    /// EP3: Invalid customer name - should throw BookingValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidCustomerName_ShouldThrowBookingValidationException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;
        var created = await _service.CreateAsync(booking);
        created.CustomerName = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingValidationException>(
            () => _service.UpdateAsync(created)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw BookingValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task UpdateAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingValidationException>(
            () => _service.UpdateAsync(booking)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return true and delete
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_ValidId_ShouldDeleteBooking()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;
        var created = await _service.CreateAsync(booking);

        // Act
        var result = await _service.DeleteAsync(created.Id);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw BookingNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingNotFoundException>(
            () => _service.DeleteAsync(nonExistentId)
        );
        Assert.Equal(nonExistentId, exception.BookingId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw BookingValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    /// <summary>
    /// EP4: Delete and verify data removed - GetById should throw
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_VerifyDataRemoved_ShouldThrowNotFoundException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;
        var created = await _service.CreateAsync(booking);

        // Act
        await _service.DeleteAsync(created.Id);

        // Assert - Verify data is completely removed
        var exception = await Assert.ThrowsAsync<ServiceBookingNotFoundException>(
            () => _service.GetByIdAsync(created.Id)
        );
        Assert.Equal(created.Id, exception.BookingId);
        
        // Also verify ExistsAsync returns false
        var exists = await _service.ExistsAsync(created.Id);
        Assert.False(exists);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Existing ID - should return true
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_BookingExists_ShouldReturnTrue()
    {
        // Arrange
        var booking = new BookingBuilder().WithId(0).WithCustomerName("John Doe").WithCustomerEmail("john@example.com").Build();
        var created = await _service.CreateAsync(booking);

        // Act
        var result = await _service.ExistsAsync(created.Id);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_BookingNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var result = await _service.ExistsAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: ID after deletion - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_AfterDeletion_ShouldReturnFalse()
    {
        // Arrange
        var booking = new BookingBuilder().WithId(0).WithCustomerName("John Doe").WithCustomerEmail("john@example.com").Build();
        var created = await _service.CreateAsync(booking);
        await _service.DeleteAsync(created.Id);

        // Act
        var result = await _service.ExistsAsync(created.Id);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw BookingValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task ExistsAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceBookingValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: Empty database - should return 0
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_EmptyDatabase_ShouldReturnZero()
    {
        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// EP2: Single booking - should return 1
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_SingleBooking_ShouldReturnOne()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        booking.Id = 0;
        await _service.CreateAsync(booking);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(1, result);
    }

    /// <summary>
    /// EP3: Multiple bookings - should return correct count
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_MultipleBookings_ShouldReturnCorrectCount()
    {
        // Arrange
        var bookings = BookingMother.CreateBookingList(7);
        foreach (var booking in bookings)
        {
            booking.Id = 0;
            await _service.CreateAsync(booking);
        }

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(7, result);
    }

    #endregion
}
