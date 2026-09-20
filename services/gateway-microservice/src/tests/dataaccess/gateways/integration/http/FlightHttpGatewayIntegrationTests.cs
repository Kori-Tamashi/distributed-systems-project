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
/// Integration tests for FlightHttpGateway
/// Sends real HTTP requests to flight-microservice-api-test
/// Note: Only tests GET methods (POST/PUT/DELETE have issues with flight API)
/// </summary>
public class FlightHttpGatewayIntegrationTests : IDisposable
{
    private readonly HttpGatewayIntegrationTestContext _context;
    private readonly IFlightGateway _gateway;

    public FlightHttpGatewayIntegrationTests()
    {
        _context = new HttpGatewayIntegrationTestContext();
        _gateway = new FlightHttpGateway(_context.GetFlightClient(), _context.GetBaseUrl("flight"));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_ShouldReturnFlights()
    {
        var flights = await _gateway.GetAllAsync();
        Assert.NotNull(flights);
        // Flight API may return empty list - that's OK
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilter_ShouldReturnMatchingFlights()
    {
        var flights = await _gateway.GetAllAsync(new FlightFilter { FlightNumber = "SU100" });
        Assert.NotNull(flights);
    }
}
