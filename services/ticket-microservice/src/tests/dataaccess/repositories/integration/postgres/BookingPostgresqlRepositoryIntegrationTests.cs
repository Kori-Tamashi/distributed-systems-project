using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using core.domain;
using core.enums;
using core.exceptions.dataaccess.repositories;
using core.filters;
using core.interfaces.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.repositories.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.contexts.postgres;
using tests.fixtures.mothers;
using DotNetEnv;
using Xunit;

using BookingDomain = core.domain.Booking;

namespace tests.dataaccess.repositories.integration.postgres;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for BookingPostgresqlRepository
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_tickets).
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetByIdAsync Tests:
///    EP1: Valid existing ID (normal case) - booking exists in database
///    EP2: Non-existing ID (edge case) - booking not found, should throw BookingNotFoundException
///    EP3: Booking with null optional fields (edge case) - booking with minimal data
///    EP4: Booking with all fields populated (normal case) - booking with full data
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single booking (normal case) - should return list with one booking
///    EP3: Multiple bookings (normal case) - should return all bookings
///    EP4: Filter by customer name (normal case) - should return matching bookings
///    EP5: Filter by status (normal case) - should return bookings with specific status
///    EP6: Filter by date range (normal case) - should return bookings in range
///    EP7: Filter with no matches (edge case) - should return empty list
///    EP8: Null filter (normal case) - should return all bookings
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid booking with all fields (normal case) - should create successfully
///    EP2: Valid booking with null optional fields (edge case) - should create successfully
///    EP3: Booking with zero price (boundary case) - should create successfully
///    EP4: Duplicate ID (error case) - should throw BookingAlreadyExistsException
///    EP5: Booking with max price value (boundary case) - should create successfully
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Update with null optional fields (edge case) - should update successfully
///    EP3: Non-existing ID (error case) - should throw BookingNotFoundException
///    EP4: Update price to boundary values (boundary case) - should update successfully
///    EP5: Update status (normal case) - should update successfully
/// 
/// 5. DeleteAsync Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: Delete and verify data removed (verification case) - data should be gone
/// 
/// 6. ExistsAsync Tests:
///    EP1: Existing ID (normal case) - should return true
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: ID after deletion (verification case) - should return false
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
/// </summary>
public class BookingPostgresqlRepositoryIntegrationTests : IDisposable
{
    private readonly TestPostgresDatabaseContext _context;
    private readonly IBookingRepository _repository;
    private readonly List<BookingDomain> _createdBookings;

    static BookingPostgresqlRepositoryIntegrationTests()
    {
        // Load .env file to get TEST_POSTGRESQL_* environment variables
        DotNetEnv.Env.Load();
    }

    public BookingPostgresqlRepositoryIntegrationTests()
    {
        // Create test database context
        _context = new TestPostgresDatabaseContext();
        _context.EnsureDatabaseDeleted();
        _context.EnsureDatabaseCreated();
        
        // Create repositories using the test database connection
        _repository = new BookingPostgresqlRepository(_context);
        
        // Track created bookings for cleanup
        _createdBookings = new List<BookingDomain>();
    }

    #region IDisposable Implementation

    public void Dispose()
    {
        try
        {
            // Clean up all data
            var bookings = _context.Bookings.ToList();
            if (bookings.Any())
            {
                _context.Bookings.RemoveRange(bookings);
                _context.SaveChanges();
            }
        }
        catch
        {
            // Ignore errors during cleanup (database might not exist)
        }
        finally
        {
            _context?.Dispose();
        }
    }

    #endregion

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - booking exists in database
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_BookingExists_ShouldReturnBooking()
    {
        // Arrange
        var expectedBooking = BookingMother.CreateValidBooking();
        await _repository.CreateAsync(expectedBooking);
        _createdBookings.Add(expectedBooking);

        // Act
        var actualBooking = await _repository.GetByIdAsync(expectedBooking.Id);

        // Assert
        Assert.NotNull(actualBooking);
        Assert.Equal(expectedBooking.Id, actualBooking.Id);
        Assert.Equal(expectedBooking.BookingReference, actualBooking.BookingReference);
        Assert.Equal(expectedBooking.CustomerName, actualBooking.CustomerName);
        Assert.Equal(expectedBooking.CustomerEmail, actualBooking.CustomerEmail);
        Assert.Equal(expectedBooking.CustomerPhone, actualBooking.CustomerPhone);
        Assert.Equal((int)expectedBooking.Status, (int)actualBooking.Status);
        Assert.Equal((int)expectedBooking.PaymentMethod, (int)actualBooking.PaymentMethod);
        Assert.Equal(expectedBooking.TotalPrice, actualBooking.TotalPrice);
    }

    /// <summary>
    /// EP2: Non-existing ID - booking not found
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var nonExistingId = 999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _repository.GetByIdAsync(nonExistingId));
        
        Assert.Equal(nonExistingId, exception.BookingId);
    }

    /// <summary>
    /// EP3: Booking with minimal data - minimal fields
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_BookingWithMinimalData_ShouldReturnBooking()
    {
        // Arrange
        var expectedBooking = BookingMother.CreateMinimalBooking();
        await _repository.CreateAsync(expectedBooking);
        _createdBookings.Add(expectedBooking);

        // Act
        var actualBooking = await _repository.GetByIdAsync(expectedBooking.Id);

        // Assert
        Assert.NotNull(actualBooking);
        Assert.Equal(expectedBooking.Id, actualBooking.Id);
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
        // Arrange (database is already empty)

        // Act
        var bookings = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(bookings);
        Assert.Empty(bookings);
    }

    /// <summary>
    /// EP2: Single booking - should return list with one booking
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SingleBooking_ShouldReturnListWithOneBooking()
    {
        // Arrange
        var expectedBooking = BookingMother.CreateValidBooking();
        await _repository.CreateAsync(expectedBooking);
        _createdBookings.Add(expectedBooking);

        // Act
        var bookings = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(bookings);
        Assert.Single(bookings);
        Assert.Equal(expectedBooking.Id, bookings[0].Id);
    }

    /// <summary>
    /// EP3: Multiple bookings - should return all bookings
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultipleBookings_ShouldReturnAllBookings()
    {
        // Arrange
        var expectedBookings = BookingMother.CreateBookingList(5);
        foreach (var booking in expectedBookings)
        {
            await _repository.CreateAsync(booking);
            _createdBookings.Add(booking);
        }

        // Act
        var bookings = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(bookings);
        Assert.Equal(5, bookings.Count);
    }

    /// <summary>
    /// EP4: Filter by customer name - should return matching bookings
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithCustomerNameFilter_ShouldReturnMatchingBookings()
    {
        // Arrange
        var bookingJohn = new BookingBuilder().WithId(1).WithCustomerName("John Doe").Build();
        var bookingJane = new BookingBuilder().WithId(2).WithCustomerName("Jane Smith").Build();
        var bookingBob = new BookingBuilder().WithId(3).WithCustomerName("Bob Johnson").Build();
        
        await _repository.CreateAsync(bookingJohn);
        await _repository.CreateAsync(bookingJane);
        await _repository.CreateAsync(bookingBob);
        _createdBookings.AddRange(new[] { bookingJohn, bookingJane, bookingBob });

        // Act
        var bookings = await _repository.GetAllAsync(new BookingFilter { CustomerName = "John" });

        // Assert
        Assert.NotNull(bookings);
        Assert.Equal(2, bookings.Count);
        Assert.All(bookings, b => Assert.Contains("John", b.CustomerName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// EP5: Filter by status - should return bookings with specific status
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithStatusFilter_ShouldReturnMatchingBookings()
    {
        // Arrange
        var confirmedBooking = new BookingBuilder().WithId(0).WithConfirmedStatus().Build();
        var cancelledBooking = new BookingBuilder().WithId(0).WithCancelledStatus().Build();
        var pendingBooking = new BookingBuilder().WithId(0).WithStatus(BookingStatus.Refunded).Build();
        
        confirmedBooking = await _repository.CreateAsync(confirmedBooking);
        cancelledBooking = await _repository.CreateAsync(cancelledBooking);
        pendingBooking = await _repository.CreateAsync(pendingBooking);
        _createdBookings.AddRange(new[] { confirmedBooking, cancelledBooking, pendingBooking });

        // Act
        var bookings = await _repository.GetAllAsync(new BookingFilter 
        { 
            Status = (int)BookingStatus.Confirmed 
        });

        // Assert
        Assert.NotNull(bookings);
        Assert.Single(bookings);
        Assert.All(bookings, b => Assert.Equal((int)BookingStatus.Confirmed, (int)b.Status));
    }

    /// <summary>
    /// EP6: Filter by date range - should return bookings in range
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithDateRangeFilter_ShouldReturnBookingsInRange()
    {
        // Arrange
        var oldBooking = new BookingBuilder().WithId(1).WithPastBookingDate().Build();
        var recentBooking1 = new BookingBuilder().WithId(2).WithBookingDate(DateTime.UtcNow.AddDays(-10)).Build();
        var recentBooking2 = new BookingBuilder().WithId(3).WithBookingDate(DateTime.UtcNow.AddDays(-5)).Build();
        
        await _repository.CreateAsync(oldBooking);
        await _repository.CreateAsync(recentBooking1);
        await _repository.CreateAsync(recentBooking2);
        _createdBookings.AddRange(new[] { oldBooking, recentBooking1, recentBooking2 });

        var startDate = DateTime.UtcNow.AddDays(-15);
        var endDate = DateTime.UtcNow.AddDays(-3);

        // Act
        var bookings = await _repository.GetAllAsync(new BookingFilter 
        { 
            MinBookingDate = startDate, 
            MaxBookingDate = endDate 
        });

        // Assert
        Assert.NotNull(bookings);
        Assert.Equal(2, bookings.Count);
        Assert.All(bookings, b => Assert.InRange(b.BookingDate, startDate, endDate));
    }

    /// <summary>
    /// EP7: Filter with no matches - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilterNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        await _repository.CreateAsync(booking);
        _createdBookings.Add(booking);

        // Act
        var bookings = await _repository.GetAllAsync(new BookingFilter { CustomerName = "NONEXISTENT" });

        // Assert
        Assert.NotNull(bookings);
        Assert.Empty(bookings);
    }

    /// <summary>
    /// EP8: Null filter - should return all bookings
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithNullFilter_ShouldReturnAllBookings()
    {
        // Arrange
        var bookings = BookingMother.CreateBookingList(3);
        foreach (var booking in bookings)
        {
            await _repository.CreateAsync(booking);
            _createdBookings.Add(booking);
        }

        // Act
        var result = await _repository.GetAllAsync(filter: null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
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
        var bookingToCreate = BookingMother.CreateValidBooking();

        // Act
        var createdBooking = await _repository.CreateAsync(bookingToCreate);
        _createdBookings.Add(createdBooking);

        // Assert
        Assert.NotNull(createdBooking);
        Assert.NotEqual(0, createdBooking.Id);
        Assert.Equal(bookingToCreate.BookingReference, createdBooking.BookingReference);
        Assert.Equal(bookingToCreate.CustomerName, createdBooking.CustomerName);
        Assert.Equal(bookingToCreate.CustomerEmail, createdBooking.CustomerEmail);
        Assert.Equal(bookingToCreate.CustomerPhone, createdBooking.CustomerPhone);
        Assert.Equal((int)bookingToCreate.Status, (int)createdBooking.Status);
        Assert.Equal((int)bookingToCreate.PaymentMethod, (int)createdBooking.PaymentMethod);
        Assert.Equal(bookingToCreate.TotalPrice, createdBooking.TotalPrice);
    }

    /// <summary>
    /// EP2: Valid booking with minimal fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_MinimalBooking_ShouldCreateBooking()
    {
        // Arrange
        var bookingToCreate = BookingMother.CreateMinimalBooking();

        // Act
        var createdBooking = await _repository.CreateAsync(bookingToCreate);
        _createdBookings.Add(createdBooking);

        // Assert
        Assert.NotNull(createdBooking);
        Assert.NotEqual(0, createdBooking.Id);
    }

    /// <summary>
    /// EP3: Booking with zero price - boundary case
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_BookingWithZeroPrice_ShouldCreateBooking()
    {
        // Arrange
        var bookingToCreate = BookingMother.CreateBookingWithZeroPrice();

        // Act
        var createdBooking = await _repository.CreateAsync(bookingToCreate);
        _createdBookings.Add(createdBooking);

        // Assert
        Assert.NotNull(createdBooking);
        Assert.Equal(0, createdBooking.TotalPrice);
    }

    /// <summary>
    /// EP4: Duplicate ID - should throw BookingAlreadyExistsException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_DuplicateId_ShouldThrowBookingAlreadyExistsException()
    {
        // Arrange
        var booking1 = new BookingBuilder().WithId(1).WithBookingReference("BK001").Build();
        var booking2 = new BookingBuilder().WithId(1).WithBookingReference("BK002").Build();
        
        await _repository.CreateAsync(booking1);
        _createdBookings.Add(booking1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingAlreadyExistsException>(
            () => _repository.CreateAsync(booking2));
        
        Assert.Equal(1, exception.BookingId);
    }

    /// <summary>
    /// EP5: Booking with max price value - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_BookingWithMaxPrice_ShouldCreateBooking()
    {
        // Arrange
        var bookingToCreate = BookingMother.CreateBookingWithMaxPrice();

        // Act
        var createdBooking = await _repository.CreateAsync(bookingToCreate);
        _createdBookings.Add(createdBooking);

        // Assert
        Assert.NotNull(createdBooking);
        Assert.Equal(int.MaxValue, createdBooking.TotalPrice);
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
        await _repository.CreateAsync(booking);
        _createdBookings.Add(booking);

        booking.CustomerName = "Updated Name";
        booking.CustomerEmail = "updated@example.com";
        booking.TotalPrice = 50000;
        booking.Status = BookingStatus.Cancelled;

        // Act
        var updatedBooking = await _repository.UpdateAsync(booking);

        // Assert
        Assert.NotNull(updatedBooking);
        Assert.Equal("Updated Name", updatedBooking.CustomerName);
        Assert.Equal("updated@example.com", updatedBooking.CustomerEmail);
        Assert.Equal(50000, updatedBooking.TotalPrice);
        Assert.Equal(BookingStatus.Cancelled, updatedBooking.Status);
    }

    /// <summary>
    /// EP2: Update price to zero - boundary case
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_UpdatePriceToZero_ShouldUpdateBooking()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        await _repository.CreateAsync(booking);
        _createdBookings.Add(booking);

        booking.TotalPrice = 0;

        // Act
        var updatedBooking = await _repository.UpdateAsync(booking);

        // Assert
        Assert.NotNull(updatedBooking);
        Assert.Equal(0, updatedBooking.TotalPrice);
    }

    /// <summary>
    /// EP3: Non-existing ID - should throw BookingNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_BookingNotFound_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var booking = new BookingBuilder().WithId(999).WithBookingReference("NonExistent").Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _repository.UpdateAsync(booking));
        
        Assert.Equal(999, exception.BookingId);
    }

    /// <summary>
    /// EP4: Update status - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_UpdateStatus_ShouldUpdateBooking()
    {
        // Arrange
        var booking = new BookingBuilder().WithId(0).WithBookingReference("BK001").WithConfirmedStatus().Build();
        booking = await _repository.CreateAsync(booking);
        _createdBookings.Add(booking);

        booking.Status = BookingStatus.Refunded;

        // Act
        var updatedBooking = await _repository.UpdateAsync(booking);

        // Assert
        Assert.NotNull(updatedBooking);
        Assert.Equal(BookingStatus.Refunded, updatedBooking.Status);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return true and delete
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_BookingExists_ShouldReturnTrueAndDelete()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        await _repository.CreateAsync(booking);
        _createdBookings.Add(booking);

        // Act
        var result = await _repository.DeleteAsync(booking.Id);

        // Assert
        Assert.True(result);
        
        // Verify booking is deleted
        var exists = await _repository.ExistsAsync(booking.Id);
        Assert.False(exists);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_BookingNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var result = await _repository.DeleteAsync(nonExistingId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Delete and verify data removed - should throw NotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_VerifyDataRemoved_ShouldThrowNotFoundException()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        await _repository.CreateAsync(booking);
        _createdBookings.Add(booking);

        // Act
        await _repository.DeleteAsync(booking.Id);

        // Assert
        await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _repository.GetByIdAsync(booking.Id));
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
        var booking = BookingMother.CreateValidBooking();
        await _repository.CreateAsync(booking);
        _createdBookings.Add(booking);

        // Act
        var exists = await _repository.ExistsAsync(booking.Id);

        // Assert
        Assert.True(exists);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_BookingNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var exists = await _repository.ExistsAsync(nonExistingId);

        // Assert
        Assert.False(exists);
    }

    /// <summary>
    /// EP3: ID after deletion - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_AfterDeletion_ShouldReturnFalse()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        await _repository.CreateAsync(booking);
        _createdBookings.Add(booking);
        
        await _repository.DeleteAsync(booking.Id);

        // Act
        var exists = await _repository.ExistsAsync(booking.Id);

        // Assert
        Assert.False(exists);
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
        // Arrange (database is already empty)

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(0, count);
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
        await _repository.CreateAsync(booking);
        _createdBookings.Add(booking);

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(1, count);
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
            await _repository.CreateAsync(booking);
            _createdBookings.Add(booking);
        }

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(7, count);
    }

    #endregion
}
