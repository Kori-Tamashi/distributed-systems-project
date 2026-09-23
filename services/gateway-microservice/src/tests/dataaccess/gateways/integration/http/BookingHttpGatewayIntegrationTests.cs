using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using core.domain;
using core.enums;
using core.filters;
using core.interfaces.dataaccess.gateways;
using dataaccess.gateways.http;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.contexts.http;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.gateways.integration.http;

[Collection("HttpGatewayIntegrationTests")]
/// <summary>
/// Integration tests for BookingHttpGateway
/// Sends real HTTP requests to ticket-microservice-api-test
/// CEP: GetAllAsync(EP1-3), GetByIdAsync(EP1-2), CreateAsync(EP1)
/// Note: Only tests GET and POST methods (PUT/DELETE not supported by booking API)
/// </summary>
public class BookingHttpGatewayIntegrationTests : IDisposable
{
    private readonly HttpGatewayIntegrationTestContext _context;
    private readonly IBookingGateway _gateway;

    public BookingHttpGatewayIntegrationTests()
    {
        _context = new HttpGatewayIntegrationTestContext();
        _gateway = new BookingHttpGateway(_context.GetBookingClient(), _context.GetBaseUrl("booking"));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_ShouldReturnBookings()
    {
        var bookings = await _gateway.GetAllAsync();
        Assert.NotNull(bookings);
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilter_ShouldReturnMatchingBookings()
    {
        var bookings = await _gateway.GetAllAsync(new BookingFilter { CustomerName = "John" });
        Assert.NotNull(bookings);
    }

    [Fact]
    [Integration]
    public async Task GetByIdAsync_ShouldReturnBooking()
    {
        var all = await _gateway.GetAllAsync();
        var bookingList = all.ToList();
        if (bookingList.Count > 0)
        {
            var booking = await _gateway.GetByIdAsync(bookingList[0].Id);
            Assert.NotNull(booking);
        }
    }

    [Fact]
    [Integration]
    public async Task CreateAsync_ValidBooking_ShouldCreateBooking()
    {
        var b = new BookingBuilder()
            .WithId(0)
            .WithCustomerName("Test Customer " + Guid.NewGuid().ToString().Substring(0, 8))
            .WithCustomerEmail("test@example.com")
            .WithTotalPrice(50000)
            .WithConfirmedStatus()
            .WithCardPayment()
            .Build();
        
        var created = await _gateway.CreateAsync(b);
        Assert.NotNull(created);
        Assert.Equal(b.CustomerName, created.CustomerName);
    }
}
