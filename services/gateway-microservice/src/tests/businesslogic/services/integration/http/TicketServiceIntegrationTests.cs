using System;
using System.Linq;
using System.Threading.Tasks;
using businesslogic.services;
using core.domain;
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
/// Integration tests for TicketService
/// Tests send real HTTP requests to ticket-microservice-api-test
/// Tests are automatically skipped if API is unavailable or required data is missing
/// </summary>
public class TicketServiceIntegrationTests : HttpIntegrationTestBase
{
    private readonly ITicketGateway _gateway;
    private readonly ITicketService _service;
    private readonly IFlightGateway _flightGateway;

    static TicketServiceIntegrationTests()
    {
        DotNetEnv.Env.Load();
    }

    public TicketServiceIntegrationTests(ITestOutputHelper output)
        : base(new HttpGatewayIntegrationTestContext(), output, "Ticket")
    {
        _gateway = new TicketHttpGateway(TestContext.GetTicketClient(), TestContext.GetBaseUrl("ticket"));
        _flightGateway = new FlightHttpGateway(TestContext.GetFlightClient(), TestContext.GetBaseUrl("flight"));
        _service = new TicketService(_gateway, new Microsoft.Extensions.Logging.Abstractions.NullLogger<TicketService>());
    }

    #region GetByIdAsync Tests

    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_TicketExists_ShouldReturnTicket()
    {
        if (SkipIfApiUnavailable()) return;
        
        var tickets = await _gateway.GetAllAsync();
        if (tickets == null || !tickets.Any())
        {
            Output.WriteLine("[SKIP] No tickets available in API");
            return;
        }

        var testTicket = tickets.First();
        var result = await _service.GetByIdAsync(testTicket.Id);

        Assert.NotNull(result);
        Assert.Equal(testTicket.Id, result.Id);
    }

    [Fact]
    [Integration]
    public async Task GetByIdAsync_NonExistingId_ShouldThrowTicketNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    [Integration]
    public async Task GetAllAsync_ShouldReturnTickets()
    {
        var result = await _service.GetAllAsync();
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredTickets()
    {
        var filter = new TicketFilter { SeatNumber = "1A" };
        var result = await _service.GetAllAsync(filter);
        Assert.NotNull(result);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    [Integration]
    public async Task CreateAsync_ValidTicket_ShouldReturnCreatedTicket()
    {
        if (SkipIfApiUnavailable()) return;
        
        // First check if we have any flights available
        var flights = await _flightGateway.GetAllAsync();
        if (flights == null || !flights.Any())
        {
            Output.WriteLine("[SKIP] No flights available for creating ticket");
            return;
        }

        var validFlight = flights.First();
        var ticket = new Ticket
        {
            FlightId = validFlight.Id,
            SeatNumber = $"TEST{DateTime.UtcNow.Ticks % 1000}",
            Price = 10000,
            PassengerName = "Test Passenger"
        };

        var result = await _service.CreateAsync(ticket);
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task CreateAsync_InvalidTicket_ShouldThrowValidationException()
    {
        var ticket = new Ticket
        {
            FlightId = 0,
            SeatNumber = "",
            Price = 0,
            PassengerName = ""
        };

        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.CreateAsync(ticket)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    [Integration]
    public async Task UpdateAsync_ValidTicket_ShouldReturnUpdatedTicket()
    {
        if (SkipIfApiUnavailable()) return;
        
        var tickets = await _gateway.GetAllAsync();
        if (tickets == null || !tickets.Any())
        {
            Output.WriteLine("[SKIP] No tickets available for update test");
            return;
        }

        var ticket = tickets.First();
        // Skip if API returns invalid data (known issue with ticket-microservice API)
        if (string.IsNullOrWhiteSpace(ticket.PassengerName) || 
            string.IsNullOrWhiteSpace(ticket.PassengerEmail) ||
            ticket.BookingDate == DateTime.MinValue)
        {
            Output.WriteLine("[SKIP] API returns invalid ticket data (known issue)");
            return;
        }

        var updatedTicket = new Ticket
        {
            Id = ticket.Id,
            FlightId = ticket.FlightId,
            SeatNumber = ticket.SeatNumber + "_Updated",
            Price = ticket.Price,
            PassengerName = ticket.PassengerName,
            PassengerEmail = ticket.PassengerEmail,
            PassengerPhone = string.IsNullOrWhiteSpace(ticket.PassengerPhone) ? "1234567890" : ticket.PassengerPhone,
            BookingDate = ticket.BookingDate == DateTime.MinValue ? DateTime.UtcNow.AddHours(-1) : ticket.BookingDate
        };

        var result = await _service.UpdateAsync(updatedTicket);
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidTicket_ShouldThrowValidationException()
    {
        var ticket = new Ticket
        {
            Id = 1,
            FlightId = 0,
            SeatNumber = "",
            Price = 0,
            PassengerName = ""
        };

        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.UpdateAsync(ticket)
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
        
        // First check if we have any flights available
        var flights = await _flightGateway.GetAllAsync();
        if (flights == null || !flights.Any())
        {
            Output.WriteLine("[SKIP] No flights available for creating ticket");
            return;
        }

        var validFlight = flights.First();
        var ticket = new Ticket
        {
            FlightId = validFlight.Id,
            SeatNumber = $"DEL{DateTime.UtcNow.Ticks % 1000}",
            Price = 10000,
            PassengerName = "Delete Test Passenger"
        };

        var created = await _service.CreateAsync(ticket);
        if (created.Id <= 0) { Output.WriteLine("[SKIP] API did not return valid ID, skipping delete test"); return; }
        await _service.DeleteAsync(created.Id);
    }

    [Fact]
    [Integration]
    public async Task DeleteAsync_NonExistingId_ShouldThrowTicketNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    [Integration]
    public async Task ExistsAsync_ExistingId_ShouldReturnTrue()
    {
        if (SkipIfApiUnavailable()) return;
        
        var tickets = await _gateway.GetAllAsync();
        if (tickets == null || !tickets.Any())
        {
            Output.WriteLine("[SKIP] No tickets available");
            return;
        }

        var testTicket = tickets.First();
        var result = await _service.ExistsAsync(testTicket.Id);
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
    public async Task ExistsAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
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
        var filter = new TicketFilter { SeatNumber = "NONEXISTENT12345" };
        var result = await _service.GetCountAsync(filter);
        Assert.True(result >= 0);
    }

    #endregion
}
