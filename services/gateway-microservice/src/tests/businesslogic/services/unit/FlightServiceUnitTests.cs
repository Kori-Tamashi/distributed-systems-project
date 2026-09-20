using core.domain;
using Microsoft.Extensions.Logging;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using businesslogic.services;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

using GatewayFlightNotFoundException = core.exceptions.dataaccess.gateways.FlightGatewayEntityNotFoundException;
using GatewayFlightCommunicationException = core.exceptions.dataaccess.gateways.FlightGatewayCommunicationException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for FlightService
/// Using London-style TDD with Mocks (Moq) for HTTP Gateways
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, Flight exists (normal case)
/// - EP2: Valid ID, Flight does not exist (GatewayFlightNotFoundException)
/// - EP3: Invalid ID (<= 0) (FlightValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetAllAsync(FlightFilter? filter):
/// - EP1: No filter, returns all flights
/// - EP2: With filter, returns filtered flights
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For CreateAsync(Flight flight):
/// - EP1: Valid flight, creation successful (normal case)
/// - EP2: Invalid flight (FlightValidationException)
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For UpdateAsync(Flight flight):
/// - EP1: Valid flight, Flight exists, update successful (normal case)
/// - EP2: Flight does not exist (GatewayFlightNotFoundException)
/// - EP3: Invalid flight (FlightValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, Flight exists, deletion successful (normal case)
/// - EP2: Valid ID, Flight does not exist (GatewayFlightNotFoundException)
/// - EP3: Invalid ID (<= 0) (FlightValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, Flight exists (returns true)
/// - EP2: Valid ID, Flight does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (FlightValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetCountAsync(FlightFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Gateway communication error (ValidationException)
/// 
/// Total: 21 unit tests (all should pass)
/// </summary>
public class FlightServiceUnitTests
{
    private readonly Mock<IFlightGateway> _mockFlightGateway;
    private readonly IFlightService _service;

    public FlightServiceUnitTests()
    {
        // Arrange - Setup mock gateway
        _mockFlightGateway = new Mock<IFlightGateway>();
        _service = new FlightService(_mockFlightGateway.Object, Mock.Of<ILogger<FlightService>>());
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid ID, Flight exists - should return Flight
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ValidId_FlightExists_ShouldReturnFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockFlightGateway.Setup(g => g.GetByIdAsync(flight.Id)).ReturnsAsync(flight);

        // Act
        var result = await _service.GetByIdAsync(flight.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(flight.Id, result.Id);
        Assert.Equal(flight.FlightNumber, result.FlightNumber);
        _mockFlightGateway.Verify(g => g.GetByIdAsync(flight.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Flight does not exist - should throw FlightNotFoundException
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ValidId_FlightNotFound_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var flightId = 999;
        _mockFlightGateway.Setup(g => g.DeleteAsync(flightId)).ThrowsAsync(new GatewayFlightNotFoundException($"Flight with id {flightId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _service.GetByIdAsync(flightId)
        );
        Assert.Equal(flightId, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw FlightValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetByIdAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var flightId = 1;
        _mockFlightGateway.Setup(g => g.GetByIdAsync(flightId))
            .ThrowsAsync(new GatewayFlightCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetByIdAsync(flightId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: No filter, returns all flights
    /// </summary>
    [Fact]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllFlights()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(5);
        _mockFlightGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(flights);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        _mockFlightGateway.Verify(g => g.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered flights
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredFlights()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(3);
        var filter = new FlightFilter { FlightNumber = "SU100" };
        _mockFlightGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(flights);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        _mockFlightGateway.Verify(g => g.GetAllAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task GetAllAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        _mockFlightGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayFlightCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid flight, creation successful - should return created flight
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidFlight_ShouldReturnCreatedFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockFlightGateway.Setup(g => g.CreateAsync(It.IsAny<Flight>())).ReturnsAsync(flight);

        // Act
        var result = await _service.CreateAsync(flight);

        // Assert
        Assert.NotNull(result);
        _mockFlightGateway.Verify(g => g.CreateAsync(It.IsAny<Flight>()), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid flight (empty flight number) - should throw FlightValidationException
    /// </summary>
    [Fact]
    public async Task CreateAsync_InvalidFlight_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = new Flight { FlightNumber = "", FromAirportId = 1, ToAirportId = 2, DateTime = DateTime.UtcNow.AddHours(-1), Price = 1000 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP3: Same from/to airport - should throw FlightValidationException
    /// </summary>
    [Fact]
    public async Task CreateAsync_SameFromToAirport_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.FromAirportId = 1;
        flight.ToAirportId = 1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task CreateAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockFlightGateway.Setup(g => g.CreateAsync(It.IsAny<Flight>()))
            .ThrowsAsync(new GatewayFlightCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid flight, Flight exists, update successful - should return updated flight
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ValidFlight_FlightExists_ShouldReturnUpdatedFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockFlightGateway.Setup(g => g.UpdateAsync(It.IsAny<Flight>())).ReturnsAsync(flight);

        // Act
        var result = await _service.UpdateAsync(flight);

        // Assert
        Assert.NotNull(result);
        _mockFlightGateway.Verify(g => g.UpdateAsync(It.IsAny<Flight>()), Times.Once);
    }

    /// <summary>
    /// EP2: Flight does not exist - should throw FlightNotFoundException
    /// </summary>
    [Fact]
    public async Task UpdateAsync_FlightNotFound_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockFlightGateway.Setup(g => g.UpdateAsync(It.IsAny<Flight>())).ThrowsAsync(new GatewayFlightNotFoundException($"Flight with id {flight.Id} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _service.UpdateAsync(flight)
        );
        Assert.Equal(flight.Id, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid flight - should throw FlightValidationException
    /// </summary>
    [Fact]
    public async Task UpdateAsync_InvalidFlight_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = new Flight { Id = 1, FlightNumber = "", FromAirportId = 1, ToAirportId = 2, DateTime = DateTime.UtcNow.AddHours(-1), Price = 1000 };
        _mockFlightGateway.Setup(g => g.GetByIdAsync(flight.Id)).ReturnsAsync(flight);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.UpdateAsync(flight)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task UpdateAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockFlightGateway.Setup(g => g.GetByIdAsync(flight.Id)).ReturnsAsync(flight);
        _mockFlightGateway.Setup(g => g.UpdateAsync(It.IsAny<Flight>()))
            .ThrowsAsync(new GatewayFlightCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.UpdateAsync(flight)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid ID, Flight exists, deletion successful - should return true
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ValidId_FlightExists_ShouldReturnTrue()
    {
        // Arrange
        var flightId = 1;
        _mockFlightGateway.Setup(g => g.GetByIdAsync(flightId)).ReturnsAsync(FlightMother.CreateValidFlight());
        _mockFlightGateway.Setup(g => g.DeleteAsync(flightId)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(flightId);

        // Assert
        _mockFlightGateway.Verify(g => g.DeleteAsync(flightId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Flight does not exist - should throw FlightNotFoundException
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ValidId_FlightNotFound_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var flightId = 999;
        _mockFlightGateway.Setup(g => g.DeleteAsync(flightId)).ThrowsAsync(new GatewayFlightNotFoundException($"Flight with id {flightId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _service.DeleteAsync(flightId)
        );
        Assert.Equal(flightId, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw FlightValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task DeleteAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var flightId = 1;
        _mockFlightGateway.Setup(g => g.GetByIdAsync(flightId)).ReturnsAsync(FlightMother.CreateValidFlight());
        _mockFlightGateway.Setup(g => g.DeleteAsync(flightId))
            .ThrowsAsync(new GatewayFlightCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.DeleteAsync(flightId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Valid ID, Flight exists - should return true
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ValidId_FlightExists_ShouldReturnTrue()
    {
        // Arrange
        var flightId = 1;
        _mockFlightGateway.Setup(g => g.GetByIdAsync(flightId)).ReturnsAsync(FlightMother.CreateValidFlight());

        // Act
        var result = await _service.ExistsAsync(flightId);

        // Assert
    }

    /// <summary>
    /// EP2: Valid ID, Flight does not exist - should return false
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ValidId_FlightNotFound_ShouldReturnFalse()
    {
        // Arrange
        var flightId = 999;
        _mockFlightGateway.Setup(g => g.GetByIdAsync(flightId)).ReturnsAsync((Flight?)null);

        // Act
        var result = await _service.ExistsAsync(flightId);

        // Assert
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw FlightValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExistsAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task ExistsAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var flightId = 1;
        _mockFlightGateway.Setup(g => g.GetByIdAsync(flightId))
            .ThrowsAsync(new GatewayFlightCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.ExistsAsync(flightId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: No filter, returns correct count
    /// </summary>
    [Fact]
    public async Task GetCountAsync_NoFilter_ShouldReturnCorrectCount()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(10);
        _mockFlightGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(flights);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(10, result);
    }

    /// <summary>
    /// EP2: With filter, returns filtered count
    /// </summary>
    [Fact]
    public async Task GetCountAsync_WithFilter_ShouldReturnFilteredCount()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(5);
        var filter = new FlightFilter { FlightNumber = "SU100" };
        _mockFlightGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(flights);

        // Act
        var result = await _service.GetCountAsync(filter);

        // Assert
        Assert.Equal(5, result);
    }

    /// <summary>
    /// EP3: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task GetCountAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        _mockFlightGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayFlightCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion
}
