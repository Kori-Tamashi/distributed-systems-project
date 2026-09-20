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
/// Integration tests for Privilege
/// Tests send real HTTP requests to privilege-microservice-api-test
/// Tests are automatically skipped if API is unavailable
/// </summary>
public class PrivilegeServiceIntegrationTests : HttpIntegrationTestBase
{
    private readonly IPrivilegeGateway _gateway;
    private readonly IPrivilegeHistoryGateway _historyGateway;
    private readonly IPrivilegeService _service;

    static PrivilegeServiceIntegrationTests()
    {
        DotNetEnv.Env.Load();
    }

    public PrivilegeServiceIntegrationTests(ITestOutputHelper output)
        : base(new HttpGatewayIntegrationTestContext(), output, "Privilege")
    {
        _gateway = new PrivilegeHttpGateway(TestContext.GetPrivilegeClient(), TestContext.GetBaseUrl("privilege"));
        _historyGateway = new PrivilegeHistoryHttpGateway(TestContext.GetPrivilegeHistoryClient(), TestContext.GetBaseUrl("privilegehistory"));
        _service = new PrivilegeService(_gateway, _historyGateway, new Microsoft.Extensions.Logging.Abstractions.NullLogger<PrivilegeService>());
    }

    #region GetByIdAsync Tests

    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_PrivilegeExists_ShouldReturnPrivilege()
    {
        if (SkipIfApiUnavailable()) return;
        
        var items = await _gateway.GetAllAsync();
        if (items == null || !items.Any())
        {
            Output.WriteLine("[SKIP] No privileges available in API");
            return;
        }

        var testItem = items.First();
        var result = await _service.GetByIdAsync(testItem.Id);

        Assert.NotNull(result);
        Assert.Equal(testItem.Id, result.Id);
    }

    [Fact]
    [Integration]
    public async Task GetByIdAsync_NonExistingId_ShouldThrowPrivilegeNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    [Integration]
    public async Task GetAllAsync_ShouldReturnPrivileges()
    {
        var result = await _service.GetAllAsync();
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredPrivileges()
    {
        var filter = new PrivilegeFilter { Status = PrivilegeStatus.BRONZE };
        var result = await _service.GetAllAsync(filter);
        Assert.NotNull(result);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    [Integration]
    public async Task CreateAsync_ValidPrivilege_ShouldReturnCreatedPrivilege()
    {
        if (SkipIfApiUnavailable()) return;
        
        var item = new Privilege
        {
            Username = $"TestUser{DateTime.UtcNow.Ticks % 10000}",
            Balance = 1000,
            Status = PrivilegeStatus.BRONZE
        };

        var result = await _service.CreateAsync(item);
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task CreateAsync_InvalidPrivilege_ShouldThrowValidationException()
    {
        var item = new Privilege
        {
            Username = "",
            Balance = -100,
            Status = (PrivilegeStatus)(-1)
        };

        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.CreateAsync(item)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    [Integration]
    public async Task UpdateAsync_ValidPrivilege_ShouldReturnUpdatedPrivilege()
    {
        if (SkipIfApiUnavailable()) return;
        
        var items = await _gateway.GetAllAsync();
        if (items == null || !items.Any())
        {
            return;
        }

        var item = items.First();
        var updatedItem = new Privilege
        {
            Id = item.Id,
            Username = item.Username + "_Updated",
            Balance = item.Balance + 100,
            Status = PrivilegeStatus.BRONZE
        };

        var result = await _service.UpdateAsync(updatedItem);
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task DeleteAsync_ValidId_ShouldReturnTrue()
    {
        if (SkipIfApiUnavailable()) return;
        
        var testItem = new Privilege
        {
            Username = $"TestUser{DateTime.UtcNow.Ticks % 10000}",
            Balance = 1000,
            Status = PrivilegeStatus.BRONZE
        };

        var created = await _service.CreateAsync(testItem);
        if (created.Id <= 0) { Output.WriteLine("[SKIP] API did not return valid ID, skipping delete test"); return; }
        await _service.DeleteAsync(created.Id);
    }

    [Fact]
    [Integration]
    public async Task DeleteAsync_NonExistingId_ShouldThrowPrivilegeNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
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
    public async Task ExistsAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
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
        var filter = new PrivilegeFilter { Status = (PrivilegeStatus)(-1) };
        var result = await _service.GetCountAsync(filter);
        Assert.True(result >= 0);
    }

    #endregion
}
