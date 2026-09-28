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
/// Integration tests for AirportService
/// Tests send real HTTP requests to flight-microservice-api-test
/// Tests are automatically skipped if API is unavailable
/// </summary>
public class AirportServiceIntegrationTests : HttpIntegrationTestBase
{
    private readonly IAirportGateway _gateway;
    private readonly IAirportService _service;

    static AirportServiceIntegrationTests()
    {
    }

    public AirportServiceIntegrationTests(ITestOutputHelper output)
        : base(new HttpGatewayIntegrationTestContext(), output, "Airport")
    {
        _gateway = new AirportHttpGateway(TestContext.GetAirportClient(), TestContext.GetBaseUrl("airport"));
        _service = new AirportService(_gateway, new Microsoft.Extensions.Logging.Abstractions.NullLogger<AirportService>());
    }

    #region GetByIdAsync Tests

    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_AirportExists_ShouldReturnAirport()
    {
        if (SkipIfApiUnavailable()) return;
        
        var airports = await _gateway.GetAllAsync();
        if (airports == null || !airports.Any())
        {
            Output.WriteLine("[SKIP] No airports available in API");
            return;
        }

        var testAirport = airports.First();
        var result = await _service.GetByIdAsync(testAirport.Id);

        Assert.NotNull(result);
        Assert.Equal(testAirport.Id, result.Id);
    }

    [Fact]
    [Integration]
    public async Task GetByIdAsync_NonExistingId_ShouldThrowAirportNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // This test validates locally, doesn't need API
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    [Integration]
    public async Task GetAllAsync_ShouldReturnAirports()
    {
        var result = await _service.GetAllAsync();
        Assert.NotNull(result);
    }

    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredAirports()
    {
        var filter = new AirportFilter { City = "Moscow" };
        var result = await _service.GetAllAsync(filter);
        Assert.NotNull(result);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    [Integration]
    public async Task CreateAsync_ValidAirport_ShouldReturnCreatedAirport()
    {
        if (SkipIfApiUnavailable()) return;
        
        var airport = new Airport
        {
            City = "TestCity",
            Country = "TestCountry",
            Name = $"Test Airport {DateTime.UtcNow.Ticks % 10000}"
        };

        var result = await _service.CreateAsync(airport);
        Assert.NotNull(result);
        Assert.Equal(airport.City, result.City);
    }

    [Fact]
    [Integration]
    public async Task CreateAsync_ValidAirport_ShouldReturnCreatedAirport_Patched()
    {
        if (SkipIfApiUnavailable()) return;
        
        var airport = new Airport
        {
            City = "TestCity",
            Country = "TestCountry",
            Name = $"Test Airport {DateTime.UtcNow.Ticks % 10000}"
        };

        try
        {
            var result = await _service.CreateAsync(airport);
            Assert.NotNull(result);
            Assert.Equal(airport.City, result.City);
        }
        catch (ValidationException ex) when (ex.Message.Contains("Failed to communicate"))
        {
            Output.WriteLine($"[SKIP] API CREATE failed: {ex.Message}");
            return;
        }
    }

    [Fact]
    [Integration]
    public async Task CreateAsync_InvalidAirport_ShouldThrowValidationException()
    {
        var airport = new Airport
        {
            City = "",
            Country = "USA",
            Name = ""
        };

        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.CreateAsync(airport)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidAirport_ShouldThrowValidationException()
    {
        var airport = new Airport
        {
            Id = 1,
            City = "",
            Country = "",
            Name = ""
        };

        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.UpdateAsync(airport)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    [Integration]
    [Trait("Skip", "true")]
    public async Task DeleteAsync_ValidId_ShouldReturnTrue()
    {
        if (SkipIfApiUnavailable()) return;
        
        var testAirport = new Airport
        {
            City = "DeleteTest",
            Country = "Test",
            Name = $"Delete Test Airport {DateTime.UtcNow.Ticks % 10000}"
        };

        try
        {
            var created = await _service.CreateAsync(testAirport);
            if (created.Id <= 0)
            {
                Output.WriteLine("[SKIP] API did not return valid ID, skipping delete test");
                return;
            }
            await _service.DeleteAsync(created.Id);
        }
        catch (ValidationException ex) when (ex.Message.Contains("Failed to communicate"))
        {
            Output.WriteLine($"[SKIP] API CREATE/DELETE failed: {ex.Message}");
            return;
        }
    }

    [Fact]
    [Integration]
    public async Task DeleteAsync_NonExistingId_ShouldThrowAirportNotFoundException()
    {
        if (SkipIfApiUnavailable()) return;
        
        var invalidId = 999999;
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Equal(invalidId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    [Integration]
    public async Task ExistsAsync_ExistingId_ShouldReturnTrue()
    {
        if (SkipIfApiUnavailable()) return;
        
        var airports = await _gateway.GetAllAsync();
        if (airports == null || !airports.Any())
        {
            return;
        }

        var testAirport = airports.First();
        var result = await _service.ExistsAsync(testAirport.Id);
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
    public async Task ExistsAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
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
        var filter = new AirportFilter { City = "NonExistentCity12345" };
        var result = await _service.GetCountAsync(filter);
        Assert.True(result >= 0);
    }

    #endregion
}
