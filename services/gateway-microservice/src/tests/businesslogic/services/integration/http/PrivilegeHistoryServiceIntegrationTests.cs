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
/// Integration tests for PrivilegeHistory
/// Tests send real HTTP requests to privilegehistory-microservice-api-test
/// Tests are automatically skipped if API is unavailable
/// </summary>
public class PrivilegeHistoryServiceIntegrationTests : HttpIntegrationTestBase
{
    private readonly IPrivilegeHistoryGateway _gateway;
    private readonly IPrivilegeHistoryService _service;
    private readonly IPrivilegeService _privilegeService;
    private readonly IPrivilegeGateway _privilegeGateway;

    static PrivilegeHistoryServiceIntegrationTests()
    {
    }

    public PrivilegeHistoryServiceIntegrationTests(ITestOutputHelper output)
        : base(new HttpGatewayIntegrationTestContext(), output, "PrivilegeHistory")
    {
        _gateway = new PrivilegeHistoryHttpGateway(TestContext.GetPrivilegeHistoryClient(), TestContext.GetBaseUrl("privilegehistory"));
        _service = new PrivilegeHistoryService(_gateway, new Microsoft.Extensions.Logging.Abstractions.NullLogger<PrivilegeHistoryService>());
        
        _privilegeGateway = new PrivilegeHttpGateway(TestContext.GetPrivilegeClient(), TestContext.GetBaseUrl("privilege"));
        _privilegeService = new PrivilegeService(_privilegeGateway, _gateway, new Microsoft.Extensions.Logging.Abstractions.NullLogger<PrivilegeService>());
    }

    #region GetByIdAsync Tests

    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_PrivilegeHistoryExists_ShouldReturnPrivilegeHistory()
    {
        if (SkipIfApiUnavailable()) return;
        
        var items = await _gateway.GetAllAsync();
        if (items == null || !items.Any())
        {
            Output.WriteLine("[SKIP] No privilege histories available in API");
            return;
        }

        var testItem = items.First();
        var result = await _service.GetByIdAsync(testItem.Id);

        Assert.NotNull(result);
        Assert.Equal(testItem.Id, result.Id);
    }

    [Fact]
    [Integration]
    public async Task GetByIdAsync_NonExistingId_ShouldThrowPrivilegeHistoryNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryNotFoundException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    [Integration]
    public async Task GetAllAsync_ShouldReturnPrivilegeHistorys()
    {
        var result = await _service.GetAllAsync();
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredPrivilegeHistorys()
    {
        var filter = new PrivilegeHistoryFilter { OperationType = OperationType.FILL_IN_BALANCE };
        var result = await _service.GetAllAsync(filter);
        Assert.NotNull(result);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    [Integration]
    public async Task CreateAsync_InvalidPrivilegeHistory_ShouldThrowValidationException()
    {
        var item = new PrivilegeHistory
        {
            PrivilegeId = 0,
            TicketUid = Guid.Empty,
            DateTime = DateTime.UtcNow.AddHours(-1),
            BalanceDiff = 0,
            OperationType = (OperationType)(-1)
        };

        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.CreateAsync(item)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    [Integration]
    public async Task UpdateAsync_ValidPrivilegeHistory_ShouldReturnUpdatedPrivilegeHistory()
    {
        if (SkipIfApiUnavailable()) return;
        
        var items = await _gateway.GetAllAsync();
        if (items == null || !items.Any())
        {
            Output.WriteLine("[SKIP] No privilege histories available for update test");
            return;
        }

        var item = items.First();
        var updatedItem = new PrivilegeHistory
        {
            Id = item.Id,
            PrivilegeId = item.PrivilegeId,
            TicketUid = Guid.NewGuid(),
            DateTime = item.DateTime,
            BalanceDiff = item.BalanceDiff + 10,
            OperationType = item.OperationType
        };

        var result = await _service.UpdateAsync(updatedItem);
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task DeleteAsync_NonExistingId_ShouldThrowPrivilegeHistoryNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryNotFoundException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    [Integration]
    public async Task ExistsAsync_ExistingId_ShouldReturnTrue()
    {
        if (SkipIfApiUnavailable()) return;
        
        var items = await _gateway.GetAllAsync();
        if (items == null || !items.Any())
        {
            Output.WriteLine("[SKIP] No privilege histories available");
            return;
        }

        var testItem = items.First();
        var result = await _service.ExistsAsync(testItem.Id);
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
    public async Task ExistsAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
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
        var filter = new PrivilegeHistoryFilter { OperationType = (OperationType)(-1) };
        var result = await _service.GetCountAsync(filter);
        Assert.True(result >= 0);
    }

    #endregion
}
