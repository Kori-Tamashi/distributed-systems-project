using System.Net;
using core.domain;
using core.exceptions.dataaccess.gateways;
using dataaccess.gateways.http;
using dataaccess.dto.http.Airport;
using tests.fixtures.contexts.http;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.gateways.unit.http;

/// <summary>
/// Unit tests for AirportHttpGateway
/// London-style TDD with proper mocking
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// GetAllAsync:
/// - EP1: Success - returns list of airports
/// - EP2: Empty list - returns empty collection
/// 
/// GetByIdAsync:
/// - EP1: Existing airport - returns airport
/// - EP2: Non-existent airport - returns null
/// 
/// CreateAsync:
/// - EP1: Valid airport - returns created airport
/// 
/// UpdateAsync:
/// - EP1: Valid airport - returns updated airport
/// 
/// DeleteAsync:
/// - EP1: Existing airport - returns true
/// - EP2: Non-existent airport - returns false
/// 
/// Total: 8 unit tests
/// </summary>
public class AirportHttpGatewayUnitTests
{
    private const string BaseUrl = "http://localhost:8060/api/v1";

    [Fact]
    public async Task GetAllAsync_Success_ShouldReturnListOfAirports()
    {
        var airports = AirportMother.CreateAirportList(3);
        var airportDtos = airports.Select(a => new AirportDTO
        {
            Id = a.Id,
            Name = a.Name,
            City = a.City,
            Country = a.Country
        }).ToList();

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(airportDtos));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new AirportHttpGateway(client, BaseUrl);

        var result = await gateway.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ShouldReturnEmptyCollection()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(new List<AirportDTO>()));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new AirportHttpGateway(client, BaseUrl);

        var result = await gateway.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAirport_ShouldReturnAirport()
    {
        var airport = AirportMother.CreateValidAirport();
        var airportDto = new AirportDTO
        {
            Id = airport.Id,
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country
        };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(airportDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new AirportHttpGateway(client, BaseUrl);

        var result = await gateway.GetByIdAsync(airport.Id);

        Assert.NotNull(result);
        Assert.Equal(airport.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentAirport_ShouldReturnNull()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new AirportHttpGateway(client, BaseUrl);

        var result = await gateway.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidAirport_ShouldReturnCreatedAirport()
    {
        var airport = AirportMother.CreateValidAirport();
        var responseDto = new AirportDTO { Id = 0, Name = airport.Name, City = airport.City, Country = airport.Country };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto, HttpStatusCode.Created));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new AirportHttpGateway(client, BaseUrl);

        var result = await gateway.CreateAsync(airport);

        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ValidAirport_ShouldReturnUpdatedAirport()
    {
        var airport = AirportMother.CreateValidAirport();
        var responseDto = new AirportDTO { Id = airport.Id, Name = "New Name", City = airport.City, Country = airport.Country };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new AirportHttpGateway(client, BaseUrl);

        var result = await gateway.UpdateAsync(airport);

        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ExistingAirport_ShouldNotThrow()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NoContent());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new AirportHttpGateway(client, BaseUrl);

        await gateway.DeleteAsync(1);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentAirport_ShouldThrowNotFoundException()
    {
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new AirportHttpGateway(client, BaseUrl);

        await Assert.ThrowsAsync<AirportGatewayEntityNotFoundException>(() => gateway.DeleteAsync(999));
    }
}
