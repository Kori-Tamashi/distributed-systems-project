using System.Net;
using core.domain;
using core.enums;
using core.exceptions.dataaccess.gateways;
using dataaccess.gateways.http;
using dataaccess.dto.http.Privilege;
using tests.fixtures.contexts.http;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.gateways.unit.http;

/// <summary>
/// Unit tests for PrivilegeHttpGateway
/// London-style TDD with proper mocking
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// GetAllAsync:
/// - EP1: Success - returns list of privileges
/// - EP2: Empty list - returns empty collection
/// 
/// GetByIdAsync:
/// - EP1: Existing privilege - returns privilege
/// - EP2: Non-existent privilege - returns null
/// 
/// CreateAsync:
/// - EP1: Valid privilege - returns created privilege
/// 
/// UpdateAsync:
/// - EP1: Valid privilege - returns updated privilege
/// 
/// DeleteAsync:
/// - EP1: Existing privilege - returns true
/// - EP2: Non-existent privilege - returns false
/// 
/// Total: 8 unit tests
/// </summary>
public class PrivilegeHttpGatewayUnitTests
{
    private const string BaseUrl = "http://localhost:8050/api/v1";

    [Fact]
    public async Task GetAllAsync_Success_ShouldReturnListOfPrivileges()
    {
        var privileges = PrivilegeMother.CreatePrivilegeList(3);
        var privilegeDtos = privileges.Select(p => new PrivilegeDTO { Id = p.Id, Username = p.Username, Balance = p.Balance, Status = (int)p.Status }).ToList();

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(privilegeDtos));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHttpGateway(client, BaseUrl);

        var result = await gateway.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ShouldReturnEmptyCollection()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(new List<PrivilegeDTO>()));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHttpGateway(client, BaseUrl);

        var result = await gateway.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPrivilege_ShouldReturnPrivilege()
    {
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var privilegeDto = new PrivilegeDTO { Id = privilege.Id, Username = privilege.Username, Balance = privilege.Balance, Status = (int)privilege.Status };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(privilegeDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHttpGateway(client, BaseUrl);

        var result = await gateway.GetByIdAsync(privilege.Id);

        Assert.NotNull(result);
        Assert.Equal(privilege.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentPrivilege_ShouldReturnNull()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHttpGateway(client, BaseUrl);

        var result = await gateway.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidPrivilege_ShouldReturnCreatedPrivilege()
    {
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var responseDto = new PrivilegeDTO { Id = 0, Username = privilege.Username, Balance = privilege.Balance, Status = (int)privilege.Status };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto, HttpStatusCode.Created));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHttpGateway(client, BaseUrl);

        var result = await gateway.CreateAsync(privilege);

        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ValidPrivilege_ShouldReturnUpdatedPrivilege()
    {
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var responseDto = new PrivilegeDTO { Id = privilege.Id, Username = privilege.Username, Balance = 1000, Status = (int)privilege.Status };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHttpGateway(client, BaseUrl);

        var result = await gateway.UpdateAsync(privilege);

        Assert.NotNull(result);
        Assert.Equal(1000, result.Balance);
    }

    [Fact]
    public async Task DeleteAsync_ExistingPrivilege_ShouldNotThrow()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NoContent());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHttpGateway(client, BaseUrl);

        await gateway.DeleteAsync(1);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentPrivilege_ShouldThrowNotFoundException()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new PrivilegeHttpGateway(client, BaseUrl);

        await Assert.ThrowsAsync<PrivilegeGatewayEntityNotFoundException>(() => gateway.DeleteAsync(999));
    }
}
