using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using core.filters;
using core.interfaces.dataaccess.gateways;
using dataaccess.gateways.http;
using tests.config.attributes;
using tests.fixtures.contexts.http;
using Xunit;

namespace tests.dataaccess.gateways.integration.http;

[Collection("HttpGatewayIntegrationTests")]
/// <summary>
/// Integration tests for PrivilegeHttpGateway
/// Sends real HTTP requests to bonus-microservice-api-test
/// Note: Only tests GET methods (POST/PUT/DELETE have issues with privilege API)
/// </summary>
public class PrivilegeHttpGatewayIntegrationTests : IDisposable
{
    private readonly HttpGatewayIntegrationTestContext _context;
    private readonly IPrivilegeGateway _gateway;

    public PrivilegeHttpGatewayIntegrationTests()
    {
        _context = new HttpGatewayIntegrationTestContext();
        _gateway = new PrivilegeHttpGateway(_context.GetPrivilegeClient(), _context.GetBaseUrl("privilege"));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_ShouldReturnPrivileges()
    {
        var privileges = await _gateway.GetAllAsync();
        Assert.NotNull(privileges);
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilter_ShouldReturnMatchingPrivileges()
    {
        var privileges = await _gateway.GetAllAsync(new PrivilegeFilter { Username = "test" });
        Assert.NotNull(privileges);
    }

    [Fact]
    [Integration]
    public async Task GetByIdAsync_ShouldReturnPrivilege()
    {
        var all = await _gateway.GetAllAsync();
        var privilegeList = all.ToList();
        if (privilegeList.Count > 0)
        {
            var privilege = await _gateway.GetByIdAsync(privilegeList[0].Id);
            Assert.NotNull(privilege);
        }
    }
}
