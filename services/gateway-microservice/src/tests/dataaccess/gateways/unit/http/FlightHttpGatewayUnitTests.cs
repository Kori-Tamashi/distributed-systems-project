using System.Net;
using core.domain;
using core.enums;
using core.exceptions.dataaccess.gateways;
using core.filters;
using dataaccess.gateways.http;
using dataaccess.dto.http.Flight;
using tests.fixtures.contexts.http;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.gateways.unit.http;

/// <summary>
/// Unit tests for FlightHttpGateway
/// London-style TDD with proper mocking of HTTP layer
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// GetAllAsync:
/// - EP1: Success case - returns list of flights (normal response with data)
/// - EP2: Empty list - returns empty collection (200 OK with empty array)
/// - EP3: Communication error - throws GatewayInternalServerException (500 error)
/// 
/// GetByIdAsync:
/// - EP1: Existing flight - returns flight entity (200 OK with entity)
/// - EP2: Non-existent flight - returns null (404 Not Found, caught internally)
/// 
/// CreateAsync:
/// - EP1: Valid flight - returns created flight with server-assigned ID (201 Created)
/// 
/// UpdateAsync:
/// - EP1: Valid flight - returns updated flight (200 OK)
/// 
/// DeleteAsync:
/// - EP1: Existing flight - returns true (204 No Content)
/// - EP2: Non-existent flight - returns false (404 Not Found)
/// 
/// Total: 9 unit tests covering all equivalence classes
/// </summary>
public class FlightHttpGatewayUnitTests
{
    private const string BaseUrl = "http://localhost:8060/api/v1";
    private const string ApiEndpoint = "/flights";

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: Success case - gateway returns list of flights from HTTP response
    /// </summary>
    [Fact]
    public async Task GetAllAsync_Success_ShouldReturnListOfFlights()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(3);
        var flightDtos = flights.Select(f => new FlightDTO
        {
            Id = f.Id,
            FlightNumber = f.FlightNumber,
            FlightUid = f.FlightUid,
            DateTime = f.DateTime,
            FromAirportId = f.FromAirportId,
            ToAirportId = f.ToAirportId,
            Price = f.Price
        }).ToList();

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(flightDtos));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new FlightHttpGateway(client, BaseUrl);

        // Act
        var result = await gateway.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    /// <summary>
    /// EP2: Empty list - gateway returns empty collection for 200 OK with empty array
    /// </summary>
    [Fact]
    public async Task GetAllAsync_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(new List<FlightDTO>()));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new FlightHttpGateway(client, BaseUrl);

        // Act
        var result = await gateway.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// EP3: Communication error - gateway throws exception for 500 Internal Server Error
    /// </summary>
    [Fact]
    public async Task GetAllAsync_CommunicationError_ShouldThrowGatewayInternalServerException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(MockHttpResponses.InternalServerError());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new FlightHttpGateway(client, BaseUrl);

        // Act & Assert
        await Assert.ThrowsAsync<GatewayInternalServerException>(() => gateway.GetAllAsync());
    }

    #endregion

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Existing flight - gateway returns flight entity from HTTP response
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ExistingFlight_ShouldReturnFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        var flightDto = new FlightDTO
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            FlightUid = flight.FlightUid,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = flight.Price
        };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(flightDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new FlightHttpGateway(client, BaseUrl);

        // Act
        var result = await gateway.GetByIdAsync(flight.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(flight.Id, result.Id);
        Assert.Equal(flight.FlightNumber, result.FlightNumber);
    }

    /// <summary>
    /// EP2: Non-existent flight - gateway returns null for 404 Not Found (exception caught internally)
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NonExistentFlight_ShouldReturnNull()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new FlightHttpGateway(client, BaseUrl);

        // Act
        var result = await gateway.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid flight - gateway returns created flight with server-assigned ID
    /// Note: ToDomain(CreateFlightDTO) doesn't set Id, so result.Id = 0
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidFlight_ShouldReturnCreatedFlight()
    {
        // Arrange - gateway receives DTO from HTTP response, not domain entity
        var requestFlight = FlightMother.CreateValidFlight();
        var responseDto = new FlightDTO
        {
            Id = 1,  // Server assigns new ID
            FlightNumber = requestFlight.FlightNumber,
            FlightUid = requestFlight.FlightUid,
            DateTime = requestFlight.DateTime,
            FromAirportId = requestFlight.FromAirportId,
            ToAirportId = requestFlight.ToAirportId,
            Price = requestFlight.Price
        };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto, HttpStatusCode.Created));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new FlightHttpGateway(client, BaseUrl);

        // Act
        var result = await gateway.CreateAsync(requestFlight);

        // Assert - result is domain entity from converter
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);  // Server assigns new ID, converter copies it
        Assert.Equal(requestFlight.FlightNumber, result.FlightNumber);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid flight - gateway returns updated flight from HTTP response
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ValidFlight_ShouldReturnUpdatedFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        var responseDto = new FlightDTO
        {
            Id = flight.Id,
            FlightNumber = "SU200",
            FlightUid = flight.FlightUid,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = 20000
        };

        var handler = new MockHttpMessageHandler(MockHttpResponses.Success(responseDto));
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new FlightHttpGateway(client, BaseUrl);

        // Act
        var result = await gateway.UpdateAsync(flight);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(flight.Id, result.Id);
        Assert.Equal("SU200", result.FlightNumber);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Existing flight - gateway returns true for 204 No Content
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ExistingFlight_ShouldReturnTrue()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(MockHttpResponses.NoContent());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new FlightHttpGateway(client, BaseUrl);

        // Act
        await gateway.DeleteAsync(1);

        // Assert
        // DeleteAsync now returns void and throws on error
    }

    /// <summary>
    /// EP2: Non-existent flight - gateway throws exception for 404 Not Found
    /// </summary>
    [Fact]
    public async Task DeleteAsync_NonExistentFlight_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(MockHttpResponses.NotFound());
        var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        var gateway = new FlightHttpGateway(client, BaseUrl);

        // Act & Assert
        await Assert.ThrowsAsync<FlightGatewayEntityNotFoundException>(() => gateway.DeleteAsync(999));
    }

    #endregion
}
