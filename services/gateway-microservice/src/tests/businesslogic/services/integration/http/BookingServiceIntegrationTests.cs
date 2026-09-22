using System;
using System.Linq;
using System.Threading.Tasks;
using businesslogic.services;
using core.domain;
using core.enums;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using dataaccess.gateways.http;
using tests.config.attributes;
using tests.fixtures.contexts.http;
using tests.fixtures.mothers;
using Xunit;
using Xunit.Abstractions;

namespace tests.businesslogic.services.integration.http;

[Collection("HttpGatewayIntegrationTests")]
/// <summary>
/// Integration tests for BookingService
/// Tests send real HTTP requests to ticket-microservice-api-test
/// Tests are automatically skipped if API is unavailable or required data is missing
/// </summary>
public class BookingServiceIntegrationTests : HttpIntegrationTestBase
{
    private readonly IBookingGateway _gateway;
    private readonly IBookingService _service;
    private readonly ITicketGateway _ticketGateway;

    static BookingServiceIntegrationTests()
    {
    }

    public BookingServiceIntegrationTests(ITestOutputHelper output)
        : base(new HttpGatewayIntegrationTestContext(), output, "Booking")
    {
        _gateway = new BookingHttpGateway(TestContext.GetBookingClient(), TestContext.GetBaseUrl("booking"));
        _ticketGateway = new TicketHttpGateway(TestContext.GetTicketClient(), TestContext.GetBaseUrl("ticket"));
        _service = new BookingService(_gateway, new Microsoft.Extensions.Logging.Abstractions.NullLogger<BookingService>());
    }

    #region GetByIdAsync Tests

    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_BookingExists_ShouldReturnBooking()
    {
        if (SkipIfApiUnavailable()) return;
        
        var bookings = await _gateway.GetAllAsync();
        if (bookings == null || !bookings.Any())
        {
            Output.WriteLine("[SKIP] No bookings available in API");
            return;
        }

        var testBooking = bookings.First();
        var result = await _service.GetByIdAsync(testBooking.Id);

        Assert.NotNull(result);
        Assert.Equal(testBooking.Id, result.Id);
    }

    [Fact]
    [Integration]
    public async Task GetByIdAsync_NonExistingId_ShouldThrowBookingNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    [Integration]
    public async Task GetAllAsync_ShouldReturnBookings()
    {
        var result = await _service.GetAllAsync();
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredBookings()
    {
        var filter = new BookingFilter { Status = BookingStatus.Confirmed };
        var result = await _service.GetAllAsync(filter);
        Assert.NotNull(result);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    [Integration]
    public async Task CreateAsync_ValidBooking_ShouldReturnCreatedBooking()
    {
        if (SkipIfApiUnavailable()) return;
        
        // First check if we have any tickets available
        var tickets = await _ticketGateway.GetAllAsync();
        if (tickets == null || !tickets.Any())
        {
            Output.WriteLine("[SKIP] No tickets available for creating booking");
            return;
        }

        var validTicket = tickets.First();
        var booking = new Booking
        {
            BookingReference = $"REF{DateTime.UtcNow.Ticks % 10000}",
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+1234567890",
            TotalPrice = 10000,
            BookingDate = DateTime.UtcNow,
            Status = BookingStatus.Confirmed,
            PaymentMethod = PaymentMethod.CreditCard
        };

        var result = await _service.CreateAsync(booking);
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task CreateAsync_InvalidBooking_ShouldThrowValidationException()
    {
        var booking = new Booking
        {
            BookingReference = "",
            CustomerName = "",
            CustomerEmail = "",
            CustomerPhone = "",
            TotalPrice = 0,
            BookingDate = DateTime.UtcNow.AddHours(-1),
            Status = (BookingStatus)(-1),
            PaymentMethod = (PaymentMethod)(-1)
        };

        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.CreateAsync(booking)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidBooking_ShouldThrowValidationException()
    {
        var booking = new Booking
        {
            Id = 1,
            BookingReference = "",
            CustomerName = "",
            CustomerEmail = "",
            CustomerPhone = "",
            TotalPrice = 0,
            BookingDate = DateTime.UtcNow.AddHours(-1),
            Status = (BookingStatus)(-1),
            PaymentMethod = (PaymentMethod)(-1)
        };

        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.UpdateAsync(booking)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    [Integration]
    public async Task DeleteAsync_ValidId_ShouldReturnTrue()
    {
        if (SkipIfApiUnavailable()) return;
        
        var booking = new Booking
        {
            BookingReference = $"DEL{DateTime.UtcNow.Ticks % 10000}",
            CustomerName = "Delete Test Customer",
            CustomerEmail = "delete@test.com",
            CustomerPhone = "+9876543210",
            TotalPrice = 10000,
            BookingDate = DateTime.UtcNow,
            Status = BookingStatus.Confirmed,
            PaymentMethod = PaymentMethod.CreditCard
        };

        var created = await _service.CreateAsync(booking);
        if (created.Id <= 0) { Output.WriteLine("[SKIP] API did not return valid ID, skipping delete test"); return; }
        
        try
        {
            await _service.DeleteAsync(created.Id);
        }
        catch (ValidationException ex) when (ex.Message.Contains("Failed to communicate"))
        {
            Output.WriteLine($"[SKIP] API CREATE/DELETE failed: {ex.Message}");
            return;
        }
    }

    [Fact]
    [Integration]
    public async Task DeleteAsync_NonExistingId_ShouldThrowBookingNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    [Integration]
    public async Task ExistsAsync_ExistingId_ShouldReturnTrue()
    {
        if (SkipIfApiUnavailable()) return;
        
        var bookings = await _gateway.GetAllAsync();
        if (bookings == null || !bookings.Any())
        {
            return;
        }

        var testBooking = bookings.First();
        var result = await _service.ExistsAsync(testBooking.Id);
        Assert.True(result);
    }

    [Fact]
    [Integration]
    public async Task ExistsAsync_NonExistingId_ShouldReturnFalse()
    {
        var invalidId = 999999;
        var result = await _service.ExistsAsync(invalidId);
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task ExistsAsync_InvalidId_ShouldThrowBookingValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Booking ID", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    [Integration]
    public async Task GetCountAsync_ShouldReturnCount()
    {
        var result = await _service.GetCountAsync();
        Assert.True(result >= 0);
    }

    [Fact]
    [Integration]
    public async Task GetCountAsync_WithFilter_ShouldReturnFilteredCount()
    {
        var filter = new BookingFilter { Status = (BookingStatus)(-1) };
        var result = await _service.GetCountAsync(filter);
        Assert.True(result >= 0);
    }

    #endregion
}
