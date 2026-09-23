using System.Net;
using core.domain;
using core.enums;
using core.exceptions.dataaccess.gateways;
using dataaccess.gateways.http;
using dataaccess.dto.http.Booking;
using tests.fixtures.contexts.http;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.gateways.unit.http;

/// <summary>
/// Unit tests for BookingHttpGateway
/// London-style TDD with proper mocking
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// GetAllAsync:
/// - EP1: Success - returns list of bookings
/// - EP2: Empty list - returns empty collection
/// 
/// GetByIdAsync:
/// - EP1: Existing booking - returns booking
/// - EP2: Non-existent booking - returns null
/// 
/// CreateAsync:
/// - EP1: Valid booking - returns created booking
/// 
/// UpdateAsync:
/// - EP1: Valid booking - returns updated booking
/// 
/// DeleteAsync:
/// - EP1: Existing booking - returns true
/// - EP2: Non-existent booking - returns false
/// 
/// Total: 8 unit tests
/// </summary>
public class BookingHttpGatewayUnitTests
{
    private const string BaseUrl = "http://localhost:8070/api/v1";

    [Fact]
    public async Task GetAllAsync_Success_ShouldReturnListOfBookings()
    {
        var bookings = BookingMother.CreateBookingList(3);
        var bookingDtos = bookings.Select(b => new BookingDTO { Id = b.Id, BookingReference = b.BookingReference, BookingUid = b.BookingUid, CustomerName = b.CustomerName, TotalPrice = b.TotalPrice, Status = (int)b.Status, PaymentMethod = (int)b.PaymentMethod }).ToList();

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(bookingDtos));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new BookingHttpGateway(client, BaseUrl);

        var result = await gateway.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ShouldReturnEmptyCollection()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(new List<BookingDTO>()));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new BookingHttpGateway(client, BaseUrl);

        var result = await gateway.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingBooking_ShouldReturnBooking()
    {
        var booking = BookingMother.CreateValidBooking();
        var bookingDto = new BookingDTO { Id = booking.Id, BookingReference = booking.BookingReference, BookingUid = booking.BookingUid, CustomerName = booking.CustomerName, TotalPrice = booking.TotalPrice, Status = (int)booking.Status, PaymentMethod = (int)booking.PaymentMethod };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(bookingDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new BookingHttpGateway(client, BaseUrl);

        var result = await gateway.GetByIdAsync(booking.Id);

        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentBooking_ShouldReturnNull()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new BookingHttpGateway(client, BaseUrl);

        var result = await gateway.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidBooking_ShouldReturnCreatedBooking()
    {
        var booking = BookingMother.CreateValidBooking();
        var responseDto = new BookingDTO { Id = 0, BookingReference = booking.BookingReference, BookingUid = booking.BookingUid, CustomerName = booking.CustomerName, TotalPrice = booking.TotalPrice, Status = (int)booking.Status, PaymentMethod = (int)booking.PaymentMethod };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto, HttpStatusCode.Created));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new BookingHttpGateway(client, BaseUrl);

        var result = await gateway.CreateAsync(booking);

        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ValidBooking_ShouldReturnUpdatedBooking()
    {
        var booking = BookingMother.CreateValidBooking();
        var responseDto = new BookingDTO { Id = booking.Id, BookingReference = booking.BookingReference, BookingUid = booking.BookingUid, CustomerName = booking.CustomerName, TotalPrice = 50000, Status = (int)booking.Status, PaymentMethod = (int)booking.PaymentMethod };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new BookingHttpGateway(client, BaseUrl);

        var result = await gateway.UpdateAsync(booking);

        Assert.NotNull(result);
        Assert.Equal(50000, result.TotalPrice);
    }

    [Fact]
    public async Task DeleteAsync_ExistingBooking_ShouldNotThrow()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NoContent());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new BookingHttpGateway(client, BaseUrl);

        await gateway.DeleteAsync(1);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentBooking_ShouldThrowNotFoundException()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new BookingHttpGateway(client, BaseUrl);

        await Assert.ThrowsAsync<BookingGatewayEntityNotFoundException>(() => gateway.DeleteAsync(999));
    }
}
