using System.Net;
using core.domain;
using core.exceptions.dataaccess.gateways;
using dataaccess.gateways.http;
using dataaccess.dto.http.Ticket;
using tests.fixtures.contexts.http;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.gateways.unit.http;

/// <summary>
/// Unit tests for TicketHttpGateway
/// London-style TDD with proper mocking
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// GetAllAsync:
/// - EP1: Success - returns list of tickets
/// - EP2: Empty list - returns empty collection
/// 
/// GetByIdAsync:
/// - EP1: Existing ticket - returns ticket
/// - EP2: Non-existent ticket - returns null
/// 
/// CreateAsync:
/// - EP1: Valid ticket - returns created ticket
/// 
/// UpdateAsync:
/// - EP1: Valid ticket - returns updated ticket
/// 
/// DeleteAsync:
/// - EP1: Existing ticket - returns true
/// - EP2: Non-existent ticket - returns false
/// 
/// Total: 8 unit tests
/// </summary>
public class TicketHttpGatewayUnitTests
{
    private const string BaseUrl = "http://localhost:8070/api/v1";

    [Fact]
    public async Task GetAllAsync_Success_ShouldReturnListOfTickets()
    {
        var tickets = TicketMother.CreateTicketList(3);
        var ticketDtos = tickets.Select(t => new TicketDTO { Id = t.Id, TicketUid = t.TicketUid, FlightId = t.FlightId, SeatNumber = t.SeatNumber, Price = t.Price }).ToList();

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(ticketDtos));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new TicketHttpGateway(client, BaseUrl);

        var result = await gateway.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ShouldReturnEmptyCollection()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(new List<TicketDTO>()));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new TicketHttpGateway(client, BaseUrl);

        var result = await gateway.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTicket_ShouldReturnTicket()
    {
        var ticket = TicketMother.CreateValidTicket();
        var ticketDto = new TicketDTO { Id = ticket.Id, TicketUid = ticket.TicketUid, FlightId = ticket.FlightId, SeatNumber = ticket.SeatNumber, Price = ticket.Price };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(ticketDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new TicketHttpGateway(client, BaseUrl);

        var result = await gateway.GetByIdAsync(ticket.Id);

        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentTicket_ShouldReturnNull()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new TicketHttpGateway(client, BaseUrl);

        var result = await gateway.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidTicket_ShouldReturnCreatedTicket()
    {
        var ticket = TicketMother.CreateValidTicket();
        var responseDto = new TicketDTO { Id = 0, TicketUid = ticket.TicketUid, FlightId = ticket.FlightId, SeatNumber = ticket.SeatNumber, Price = ticket.Price };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto, HttpStatusCode.Created));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new TicketHttpGateway(client, BaseUrl);

        var result = await gateway.CreateAsync(ticket);

        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ValidTicket_ShouldReturnUpdatedTicket()
    {
        var ticket = TicketMother.CreateValidTicket();
        var responseDto = new TicketDTO { Id = ticket.Id, TicketUid = ticket.TicketUid, FlightId = ticket.FlightId, SeatNumber = "A1", Price = ticket.Price };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new TicketHttpGateway(client, BaseUrl);

        var result = await gateway.UpdateAsync(ticket);

        Assert.NotNull(result);
        Assert.Equal("A1", result.SeatNumber);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTicket_ShouldNotThrow()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NoContent());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new TicketHttpGateway(client, BaseUrl);

        await gateway.DeleteAsync(1);
        // DeleteAsync now returns void and throws on error
    }

    [Fact]
    public async Task DeleteAsync_NonExistentTicket_ShouldThrowNotFoundException()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new TicketHttpGateway(client, BaseUrl);

        await Assert.ThrowsAsync<TicketGatewayEntityNotFoundException>(() => gateway.DeleteAsync(999));
    }
}
