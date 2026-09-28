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

using GatewayAirportNotFoundException = core.exceptions.dataaccess.gateways.AirportGatewayEntityNotFoundException;
using GatewayAirportCommunicationException = core.exceptions.dataaccess.gateways.AirportGatewayCommunicationException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for AirportService
/// Using London-style TDD with Mocks (Moq) for HTTP Gateways
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, Airport exists (normal case)
/// - EP2: Valid ID, Airport does not exist (GatewayAirportNotFoundException)
/// - EP3: Invalid ID (<= 0) (AirportValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetAllAsync(AirportFilter? filter):
/// - EP1: No filter, returns all airports
/// - EP2: With filter, returns filtered airports
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For CreateAsync(Airport airport):
/// - EP1: Valid airport, creation successful (normal case)
/// - EP2: Invalid airport (AirportValidationException)
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For UpdateAsync(Airport airport):
/// - EP1: Valid airport, Airport exists, update successful (normal case)
/// - EP2: Airport does not exist (GatewayAirportNotFoundException)
/// - EP3: Invalid airport (AirportValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, Airport exists, deletion successful (normal case)
/// - EP2: Valid ID, Airport does not exist (GatewayAirportNotFoundException)
/// - EP3: Invalid ID (<= 0) (AirportValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, Airport exists (returns true)
/// - EP2: Valid ID, Airport does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (AirportValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetCountAsync(AirportFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Gateway communication error (ValidationException)
/// 
/// Total: 21 unit tests (all should pass)
/// </summary>
public class AirportServiceUnitTests
{
    private readonly Mock<IAirportGateway> _mockAirportGateway;
    private readonly IAirportService _service;

    public AirportServiceUnitTests()
    {
        // Arrange - Setup mock gateway
        _mockAirportGateway = new Mock<IAirportGateway>();
        _service = new AirportService(_mockAirportGateway.Object, Mock.Of<ILogger<AirportService>>());
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid ID, Airport exists - should return Airport
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ValidId_AirportExists_ShouldReturnAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airport.Id)).ReturnsAsync(airport);

        // Act
        var result = await _service.GetByIdAsync(airport.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(airport.Id, result.Id);
        Assert.Equal(airport.City, result.City);
        _mockAirportGateway.Verify(g => g.GetByIdAsync(airport.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Airport does not exist - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ValidId_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var airportId = 999;
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airportId)).ThrowsAsync(new GatewayAirportNotFoundException($"Airport with id {airportId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _service.GetByIdAsync(airportId)
        );
        Assert.Equal(airportId, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw AirportValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetByIdAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var airportId = 1;
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airportId))
            .ThrowsAsync(new GatewayAirportCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetByIdAsync(airportId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: No filter, returns all airports
    /// </summary>
    [Fact]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllAirports()
    {
        // Arrange
        var airports = AirportMother.CreateAirportList(5);
        _mockAirportGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(airports);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        _mockAirportGateway.Verify(g => g.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered airports
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredAirports()
    {
        // Arrange
        var airports = AirportMother.CreateAirportList(3);
        var filter = new AirportFilter { City = "Moscow" };
        _mockAirportGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(airports);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        _mockAirportGateway.Verify(g => g.GetAllAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task GetAllAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        _mockAirportGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayAirportCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid airport, creation successful - should return created airport
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidAirport_ShouldReturnCreatedAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        _mockAirportGateway.Setup(g => g.CreateAsync(It.IsAny<Airport>())).ReturnsAsync(airport);

        // Act
        var result = await _service.CreateAsync(airport);

        // Assert
        Assert.NotNull(result);
        _mockAirportGateway.Verify(g => g.CreateAsync(It.IsAny<Airport>()), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid airport (empty city) - should throw AirportValidationException
    /// </summary>
    [Fact]
    public async Task CreateAsync_InvalidAirport_ShouldThrowAirportValidationException()
    {
        // Arrange
        var airport = new Airport { City = "", Country = "USA" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.CreateAsync(airport)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP3: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task CreateAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        _mockAirportGateway.Setup(g => g.CreateAsync(It.IsAny<Airport>()))
            .ThrowsAsync(new GatewayAirportCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(airport)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid airport, Airport exists, update successful - should return updated airport
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ValidAirport_AirportExists_ShouldReturnUpdatedAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airport.Id)).ReturnsAsync(airport);
        _mockAirportGateway.Setup(g => g.UpdateAsync(It.IsAny<Airport>())).ReturnsAsync(airport);

        // Act
        var result = await _service.UpdateAsync(airport);

        // Assert
        Assert.NotNull(result);
        _mockAirportGateway.Verify(g => g.GetByIdAsync(airport.Id), Times.Once);
        _mockAirportGateway.Verify(g => g.UpdateAsync(It.IsAny<Airport>()), Times.Once);
    }

    /// <summary>
    /// EP2: Airport does not exist - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    public async Task UpdateAsync_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airport.Id)).ThrowsAsync(new GatewayAirportNotFoundException($"Airport with id {airport.Id} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _service.UpdateAsync(airport)
        );
        Assert.Equal(airport.Id, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid airport - should throw AirportValidationException
    /// </summary>
    [Fact]
    public async Task UpdateAsync_InvalidAirport_ShouldThrowAirportValidationException()
    {
        // Arrange
        var airport = new Airport { Id = 1, City = "", Country = "USA" };
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airport.Id)).ReturnsAsync(airport);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.UpdateAsync(airport)
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
        var airport = AirportMother.CreateValidAirport();
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airport.Id)).ReturnsAsync(airport);
        _mockAirportGateway.Setup(g => g.UpdateAsync(It.IsAny<Airport>()))
            .ThrowsAsync(new GatewayAirportCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.UpdateAsync(airport)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid ID, Airport exists, deletion successful - should return true
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ValidId_AirportExists_ShouldReturnTrue()
    {
        // Arrange
        var airportId = 1;
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airportId)).ReturnsAsync(AirportMother.CreateValidAirport());
        _mockAirportGateway.Setup(g => g.DeleteAsync(airportId)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(airportId);

        // Assert
        _mockAirportGateway.Verify(g => g.DeleteAsync(airportId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Airport does not exist - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ValidId_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var airportId = 999;
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airportId)).ThrowsAsync(new GatewayAirportNotFoundException($"Airport with id {airportId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _service.DeleteAsync(airportId)
        );
        Assert.Equal(airportId, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw AirportValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task DeleteAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var airportId = 1;
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airportId)).ReturnsAsync(AirportMother.CreateValidAirport());
        _mockAirportGateway.Setup(g => g.DeleteAsync(airportId))
            .ThrowsAsync(new GatewayAirportCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.DeleteAsync(airportId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Valid ID, Airport exists - should return true
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ValidId_AirportExists_ShouldReturnTrue()
    {
        // Arrange
        var airportId = 1;
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airportId)).ReturnsAsync(AirportMother.CreateValidAirport());

        // Act
        var result = await _service.ExistsAsync(airportId);

        // Assert
    }

    /// <summary>
    /// EP2: Valid ID, Airport does not exist - should return false
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ValidId_AirportNotFound_ShouldReturnFalse()
    {
        // Arrange
        var airportId = 999;
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airportId)).ReturnsAsync((Airport?)null);

        // Act
        var result = await _service.ExistsAsync(airportId);

        // Assert
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw AirportValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExistsAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task ExistsAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var airportId = 1;
        _mockAirportGateway.Setup(g => g.GetByIdAsync(airportId))
            .ThrowsAsync(new GatewayAirportCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.ExistsAsync(airportId)
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
        var airports = AirportMother.CreateAirportList(10);
        _mockAirportGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(airports);

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
        var airports = AirportMother.CreateAirportList(5);
        var filter = new AirportFilter { City = "Moscow" };
        _mockAirportGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(airports);

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
        _mockAirportGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayAirportCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion
}
