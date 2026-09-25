using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using core.domain;
using core.filters;
using core.interfaces.dataaccess.gateways;
using dataaccess.gateways.http;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.contexts.http;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.gateways.integration.http;

[Collection("HttpGateway IntegrationTests")]
/// <summary>
/// Integration tests for TicketHttpGateway (per lab2-template v1 spec)
/// Sends real HTTP requests to ticket-microservice-api-test
/// CEP: GetAllAsync(EP1-3), GetByIdAsync(EP1-2), CreateAsync(EP1)
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
        var tickets = await _gateway.GetAllAsync(new TicketFilter { FlightNumber = "AFL031" });
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
        var t = new TicketBuilder()
            .WithId(0)
            .WithUsername("test_passenger")
            .WithFlightNumber("AFL031")
            .WithPrice(15000)
            .WithPaidStatus()
            .Build();
        
        var created = await _gateway.CreateAsync(t);
        Assert.NotNull(created);
        Assert.Equal(t.Username, created.Username);
    }
}
