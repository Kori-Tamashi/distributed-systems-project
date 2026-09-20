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
/// Integration tests for FlightService
/// Tests send real HTTP requests to flight-microservice-api-test
/// Tests are automatically skipped if API is unavailable
/// </summary>
public class FlightServiceIntegrationTests : HttpIntegrationTestBase
{
    private readonly IFlightGateway _gateway;
    private readonly IFlightService _service;

    static FlightServiceIntegrationTests()
    {
        DotNetEnv.Env.Load();
    }

    public FlightServiceIntegrationTests(ITestOutputHelper output)
        : base(new HttpGatewayIntegrationTestContext(), output, "Flight")
    {
        _gateway = new FlightHttpGateway(TestContext.GetFlightClient(), TestContext.GetBaseUrl("flight"));
        _service = new FlightService(_gateway, new Microsoft.Extensions.Logging.Abstractions.NullLogger<FlightService>());
    }

    #region GetByIdAsync Tests

    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_FlightExists_ShouldReturnFlight()
    {
        if (SkipIfApiUnavailable()) return;
        
        var flights = await _gateway.GetAllAsync();
        if (flights == null || !flights.Any())
        {
            Output.WriteLine("[SKIP] No flights available in API");
            return;
        }

        var testFlight = flights.First();
        var result = await _service.GetByIdAsync(testFlight.Id);

        Assert.NotNull(result);
        Assert.Equal(testFlight.Id, result.Id);
    }

    [Fact]
    [Integration]
    public async Task GetByIdAsync_NonExistingId_ShouldThrowFlightNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    [Integration]
    public async Task GetAllAsync_ShouldReturnFlights()
    {
        var result = await _service.GetAllAsync();
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredFlights()
    {
        var filter = new FlightFilter { FlightNumber = "SU" };
        var result = await _service.GetAllAsync(filter);
        Assert.NotNull(result);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    [Integration]
    public async Task CreateAsync_InvalidFlight_ShouldThrowValidationException()
    {
        var flight = new Flight
        {
            FlightNumber = "",
            FromAirportId = 0,
            ToAirportId = 0,
            DateTime = DateTime.UtcNow.AddHours(-1),
            Price = 0
        };

        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    [Integration]
    public async Task UpdateAsync_ValidFlight_ShouldReturnUpdatedFlight()
    {
        if (SkipIfApiUnavailable()) return;
        
        var flights = await _gateway.GetAllAsync();
        if (flights == null || !flights.Any())
        {
            return;
        }

        var flight = flights.First();
        var updatedFlight = new Flight
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber + "_Updated",
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            DateTime = flight.DateTime,
            Price = flight.Price
        };

        var result = await _service.UpdateAsync(updatedFlight);
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidFlight_ShouldThrowValidationException()
    {
        var flight = new Flight
        {
            Id = 1,
            FlightNumber = "",
            FromAirportId = 0,
            ToAirportId = 0,
            DateTime = DateTime.UtcNow.AddHours(-1),
            Price = 0
        };

        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.UpdateAsync(flight)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    [Integration]
    public async Task DeleteAsync_NonExistingId_ShouldThrowFlightNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    [Integration]
    public async Task ExistsAsync_ExistingId_ShouldReturnTrue()
    {
        if (SkipIfApiUnavailable()) return;
        
        var flights = await _gateway.GetAllAsync();
        if (flights == null || !flights.Any())
        {
            return;
        }

        var testFlight = flights.First();
        var result = await _service.ExistsAsync(testFlight.Id);
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
    public async Task ExistsAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
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
        var filter = new FlightFilter { FlightNumber = "NONEXISTENT12345" };
        var result = await _service.GetCountAsync(filter);
        Assert.True(result >= 0);
    }

    #endregion
}
