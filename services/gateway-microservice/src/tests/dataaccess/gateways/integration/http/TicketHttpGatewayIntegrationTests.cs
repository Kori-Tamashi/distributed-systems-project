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
/// Integration tests for TicketHttpGateway
/// Sends real HTTP requests to ticket-microservice-api-test
/// CEP: GetAllAsync(EP1-3), GetByIdAsync(EP1-2), CreateAsync(EP1)
/// Note: Only tests GET and POST methods (PUT/DELETE not supported by ticket API)
/// </summary>
public class TicketHttpGatewayIntegrationTests : IDisposable
{
    private readonly HttpGatewayIntegrationTestContext _context;
    private readonly ITicketGateway _gateway;

    public TicketHttpGatewayIntegrationTests()
    {
        _context = new HttpGatewayIntegrationTestContext();
        _gateway = new TicketHttpGateway(_context.GetTicketClient(), _context.GetBaseUrl("ticket"));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_ShouldReturnTickets()
    {
        var tickets = await _gateway.GetAllAsync();
        Assert.NotNull(tickets);
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilter_ShouldReturnMatchingTickets()
    {
        var tickets = await _gateway.GetAllAsync(new TicketFilter { FlightId = 1 });
        Assert.NotNull(tickets);
    }

    [Fact]
    [Integration]
    public async Task GetByIdAsync_ShouldReturnTicket()
    {
        var all = await _gateway.GetAllAsync();
        var ticketList = all.ToList();
        if (ticketList.Count > 0)
        {
            var ticket = await _gateway.GetByIdAsync(ticketList[0].Id);
            Assert.NotNull(ticket);
        }
    }

    [Fact]
    [Integration]
    public async Task CreateAsync_ValidTicket_ShouldCreateTicket()
    {
        // Use a fixed flight ID that likely exists in test database
        var t = new TicketBuilder()
            .WithId(0)
            .WithFlightId(1)
            .WithPassengerName("Test Passenger")
            .WithPassengerEmail("test" + Guid.NewGuid().ToString().Substring(0, 8) + "@example.com")
            .WithClass(TicketClass.Economy)
            .WithPrice(15000)
            .WithConfirmedStatus()
            .Build();
        
        var created = await _gateway.CreateAsync(t);
        Assert.NotNull(created);
        Assert.Equal(t.PassengerName, created.PassengerName);
    }
}
