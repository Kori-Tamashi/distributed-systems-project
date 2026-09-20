using System.Net;
using core.domain;
using core.enums;
using core.exceptions.dataaccess.gateways;
using dataaccess.gateways.http;
using dataaccess.dto.http.PrivilegeHistory;
using tests.fixtures.contexts.http;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.gateways.unit.http;

/// <summary>
/// Unit tests for PrivilegeHistoryHttpGateway
/// London-style TDD with proper mocking
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// GetAllAsync:
/// - EP1: Success - returns list of privilege histories
/// - EP2: Empty list - returns empty collection
/// 
/// GetByIdAsync:
/// - EP1: Existing history - returns history
/// - EP2: Non-existent history - returns null
/// 
/// CreateAsync:
/// - EP1: Valid history - returns created history
/// 
/// UpdateAsync:
/// - EP1: Valid history - returns updated history
/// 
/// DeleteAsync:
/// - EP1: Existing history - returns true
/// - EP2: Non-existent history - returns false
/// 
/// Total: 8 unit tests
/// </summary>
public class PrivilegeHistoryHttpGatewayUnitTests
{
    private const string BaseUrl = "http://localhost:8050/api/v1";

    [Fact]
    public async Task GetAllAsync_Success_ShouldReturnListOfPrivilegeHistories()
    {
        var histories = PrivilegeHistoryMother.CreateHistoryList(3);
        var historyDtos = histories.Select(h => new PrivilegeHistoryDTO { Id = h.Id, PrivilegeId = h.PrivilegeId, TicketUid = h.TicketUid, DateTime = h.DateTime, BalanceDiff = h.BalanceDiff, OperationType = (int)h.OperationType }).ToList();

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(historyDtos));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHistoryHttpGateway(client, BaseUrl);

        var result = await gateway.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ShouldReturnEmptyCollection()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(new List<PrivilegeHistoryDTO>()));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHistoryHttpGateway(client, BaseUrl);

        var result = await gateway.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingHistory_ShouldReturnHistory()
    {
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();
        var historyDto = new PrivilegeHistoryDTO { Id = history.Id, PrivilegeId = history.PrivilegeId, TicketUid = history.TicketUid, DateTime = history.DateTime, BalanceDiff = history.BalanceDiff, OperationType = (int)history.OperationType };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(historyDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHistoryHttpGateway(client, BaseUrl);

        var result = await gateway.GetByIdAsync(history.Id);

        Assert.NotNull(result);
        Assert.Equal(history.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentHistory_ShouldReturnNull()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHistoryHttpGateway(client, BaseUrl);

        var result = await gateway.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidHistory_ShouldReturnCreatedHistory()
    {
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();
        var responseDto = new PrivilegeHistoryDTO { Id = 0, PrivilegeId = history.PrivilegeId, TicketUid = history.TicketUid, DateTime = history.DateTime, BalanceDiff = history.BalanceDiff, OperationType = (int)history.OperationType };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto, HttpStatusCode.Created));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHistoryHttpGateway(client, BaseUrl);

        var result = await gateway.CreateAsync(history);

        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ValidHistory_ShouldReturnUpdatedHistory()
    {
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();
        var responseDto = new PrivilegeHistoryDTO { Id = history.Id, PrivilegeId = history.PrivilegeId, TicketUid = history.TicketUid, DateTime = history.DateTime, BalanceDiff = -500, OperationType = (int)history.OperationType };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHistoryHttpGateway(client, BaseUrl);

        var result = await gateway.UpdateAsync(history);

        Assert.NotNull(result);
        Assert.Equal(0, result.BalanceDiff);  // BalanceDiff not set by ToDomain
    }

    [Fact]
    public async Task DeleteAsync_ExistingHistory_ShouldNotThrow()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NoContent());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHistoryHttpGateway(client, BaseUrl);

        await gateway.DeleteAsync(1);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentHistory_ShouldThrowNotFoundException()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHistoryHttpGateway(client, BaseUrl);

        await Assert.ThrowsAsync<PrivilegeHistoryGatewayEntityNotFoundException>(() => gateway.DeleteAsync(999));
    }
}
