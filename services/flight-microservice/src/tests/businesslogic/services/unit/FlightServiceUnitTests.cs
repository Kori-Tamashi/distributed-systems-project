using core.domain;
using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using businesslogic.services;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;

using RepositoryFlightAlreadyExistsException = core.exceptions.dataaccess.repositories.FlightAlreadyExistsException;
using RepositoryFlightNotFoundException = core.exceptions.dataaccess.repositories.FlightNotFoundException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for FlightService
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, Flight exists (normal case)
/// - EP2: Valid ID, Flight does not exist (FlightNotFoundException)
/// - EP3: Invalid ID (<= 0) (FlightValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetAllAsync(FlightFilter? filter):
/// - EP1: No filter, returns all flights
/// - EP2: With filter, returns filtered flights
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// For CreateAsync(Flight flight):
/// - EP1: Valid flight, creation successful (normal case)
/// - EP2: Invalid flight (FlightValidationException)
/// - EP3: Flight already exists (FlightBusinessRuleViolationException)
/// - EP4: Airport not found (AirportNotFoundException)
/// - EP5: Repository throws exception (BaseServiceException)
/// 
/// For UpdateAsync(Flight flight):
/// - EP1: Valid flight, Flight exists, update successful (normal case)
/// - EP2: Flight does not exist (FlightNotFoundException)
/// - EP3: Invalid flight (FlightValidationException)
/// - EP4: Invalid ID (FlightValidationException)
/// - EP5: Airport not found (AirportNotFoundException)
/// - EP6: Repository throws exception (BaseServiceException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, Flight exists, deletion successful (normal case)
/// - EP2: Valid ID, Flight does not exist (FlightNotFoundException)
/// - EP3: Invalid ID (<= 0) (FlightValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, Flight exists (returns true)
/// - EP2: Valid ID, Flight does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (FlightValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetCountAsync(FlightFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// Total: 27 unit tests (all should pass)
/// </summary>
public class FlightServiceUnitTests
{
    private readonly Mock<IFlightRepository> _mockFlightRepository;
    private readonly Mock<IAirportRepository> _mockAirportRepository;
    private readonly IFlightService _service;

    public FlightServiceUnitTests()
    {
        // Arrange - Setup mock repositories
        _mockFlightRepository = new Mock<IFlightRepository>();
        _mockAirportRepository = new Mock<IAirportRepository>();
        _service = new FlightService(_mockFlightRepository.Object, _mockAirportRepository.Object);
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid ID, Flight exists - should return Flight
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_FlightExists_ShouldReturnFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockFlightRepository.Setup(r => r.GetByIdAsync(flight.Id)).ReturnsAsync(flight);

        // Act
        var result = await _service.GetByIdAsync(flight.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(flight.Id, result.Id);
        Assert.Equal(flight.FlightNumber, result.FlightNumber);
        _mockFlightRepository.Verify(r => r.GetByIdAsync(flight.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Flight does not exist - should throw FlightNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_FlightNotFound_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var flightId = 999;
        _mockFlightRepository.Setup(r => r.GetByIdAsync(flightId)).ThrowsAsync(new RepositoryFlightNotFoundException(flightId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _service.GetByIdAsync(flightId)
        );
        Assert.Equal(flightId, exception.FlightId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw FlightValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Unit]
    public async Task GetByIdAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var flightId = 1;
        _mockFlightRepository.Setup(r => r.GetByIdAsync(flightId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetByIdAsync(flightId)
        );
        Assert.Contains($"Failed to get Flight with ID {flightId}", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: No filter, returns all flights - should return list of flights
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllFlights()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(5);
        _mockFlightRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(flights);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        _mockFlightRepository.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered flights - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new core.filters.FlightFilter { FlightNumber = "SU1234" };
        var filteredFlights = new List<Flight> { FlightMother.CreateValidFlight() };
        _mockFlightRepository.Setup(r => r.GetAllAsync(filter)).ReturnsAsync(filteredFlights);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockFlightRepository.Verify(r => r.GetAllAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockFlightRepository.Setup(r => r.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to get all Flights", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid flight, creation successful - should return created flight
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_ValidFlight_ShouldReturnCreatedFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = 0; // Id will be generated
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.FromAirportId)).ReturnsAsync(true);
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.ToAirportId)).ReturnsAsync(true);
        _mockFlightRepository.Setup(r => r.CreateAsync(flight)).ReturnsAsync(flight);

        // Act
        var result = await _service.CreateAsync(flight);

        // Assert
        Assert.NotNull(result);
        _mockAirportRepository.Verify(r => r.ExistsAsync(flight.FromAirportId), Times.Once);
        _mockAirportRepository.Verify(r => r.ExistsAsync(flight.ToAirportId), Times.Once);
        _mockFlightRepository.Verify(r => r.CreateAsync(flight), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid flight (empty flight number) - should throw FlightValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_InvalidFlight_EmptyFlightNumber_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = new Flight
        {
            FlightNumber = string.Empty,
            DateTime = DateTime.UtcNow.AddHours(1),
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 10000
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("Flight validation failed", exception.Message);
    }

    /// <summary>
    /// EP3: Invalid flight (past date) - should throw FlightValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_InvalidFlight_PastDate_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = FlightMother.CreatePastFlight();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("Flight validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid flight (same airports) - should throw FlightValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_InvalidFlight_SameAirports_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = FlightMother.CreateFlightWithSameAirports();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("Flight validation failed", exception.Message);
    }

    /// <summary>
    /// EP5: Invalid flight (negative price) - should throw FlightValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_InvalidFlight_NegativePrice_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = FlightMother.CreateFlightWithNegativePrice();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("Flight validation failed", exception.Message);
    }

    /// <summary>
    /// EP6: Flight already exists - should throw FlightBusinessRuleViolationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_FlightAlreadyExists_ShouldThrowFlightBusinessRuleViolationException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.FromAirportId)).ReturnsAsync(true);
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.ToAirportId)).ReturnsAsync(true);
        _mockFlightRepository.Setup(r => r.CreateAsync(flight))
            .ThrowsAsync(new RepositoryFlightAlreadyExistsException(flight.Id));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightBusinessRuleViolationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("UniqueConstraint", exception.RuleName);
        Assert.Contains("already exists", exception.Message);
    }

    /// <summary>
    /// EP7: FromAirport not found - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_FromAirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.FromAirportId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Equal(flight.FromAirportId, exception.AirportId);
    }

    /// <summary>
    /// EP8: ToAirport not found - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_ToAirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.FromAirportId)).ReturnsAsync(true);
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.ToAirportId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Equal(flight.ToAirportId, exception.AirportId);
    }

    /// <summary>
    /// EP9: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = 0;
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.FromAirportId)).ReturnsAsync(true);
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.ToAirportId)).ReturnsAsync(true);
        _mockFlightRepository.Setup(r => r.CreateAsync(flight))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("Failed to create Flight", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid flight, Flight exists, update successful - should return updated flight
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_ValidFlight_FlightExists_ShouldReturnUpdatedFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockFlightRepository.Setup(r => r.ExistsAsync(flight.Id)).ReturnsAsync(true);
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.FromAirportId)).ReturnsAsync(true);
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.ToAirportId)).ReturnsAsync(true);
        _mockFlightRepository.Setup(r => r.UpdateAsync(flight)).ReturnsAsync(flight);

        // Act
        var result = await _service.UpdateAsync(flight);

        // Assert
        Assert.NotNull(result);
        _mockFlightRepository.Verify(r => r.ExistsAsync(flight.Id), Times.Once);
        _mockAirportRepository.Verify(r => r.ExistsAsync(flight.FromAirportId), Times.Once);
        _mockAirportRepository.Verify(r => r.ExistsAsync(flight.ToAirportId), Times.Once);
        _mockFlightRepository.Verify(r => r.UpdateAsync(flight), Times.Once);
    }

    /// <summary>
    /// EP2: Flight does not exist - should throw FlightNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_FlightNotFound_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockFlightRepository.Setup(r => r.ExistsAsync(flight.Id)).ReturnsAsync(false);
        // Note: Airport validation happens before flight existence check
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.FromAirportId)).ReturnsAsync(true);
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.ToAirportId)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _service.UpdateAsync(flight)
        );
        Assert.Equal(flight.Id, exception.FlightId);
    }

    /// <summary>
    /// EP3: Invalid flight (empty flight number) - should throw FlightValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_InvalidFlight_EmptyFlightNumber_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = new Flight
        {
            Id = 1,
            FlightNumber = string.Empty,
            DateTime = DateTime.UtcNow.AddHours(1),
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 10000
        };
        _mockFlightRepository.Setup(r => r.ExistsAsync(flight.Id)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.UpdateAsync(flight)
        );
        Assert.Contains("Flight validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID (<= 0) - should throw FlightValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task UpdateAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.UpdateAsync(flight)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
    }

    /// <summary>
    /// EP5: FromAirport not found - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_FromAirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockFlightRepository.Setup(r => r.ExistsAsync(flight.Id)).ReturnsAsync(true);
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.FromAirportId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _service.UpdateAsync(flight)
        );
        Assert.Equal(flight.FromAirportId, exception.AirportId);
    }

    /// <summary>
    /// EP6: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockFlightRepository.Setup(r => r.ExistsAsync(flight.Id)).ReturnsAsync(true);
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.FromAirportId)).ReturnsAsync(true);
        _mockAirportRepository.Setup(r => r.ExistsAsync(flight.ToAirportId)).ReturnsAsync(true);
        _mockFlightRepository.Setup(r => r.UpdateAsync(flight))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.UpdateAsync(flight)
        );
        Assert.Contains($"Failed to update Flight with ID {flight.Id}", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid ID, Flight exists, deletion successful - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_FlightExists_ShouldReturnTrue()
    {
        // Arrange
        var flightId = 1;
        _mockFlightRepository.Setup(r => r.ExistsAsync(flightId)).ReturnsAsync(true);
        _mockFlightRepository.Setup(r => r.DeleteAsync(flightId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(flightId);

        // Assert
        Assert.True(result);
        _mockFlightRepository.Verify(r => r.ExistsAsync(flightId), Times.Once);
        _mockFlightRepository.Verify(r => r.DeleteAsync(flightId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Flight does not exist - should throw FlightNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_FlightNotFound_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var flightId = 999;
        _mockFlightRepository.Setup(r => r.ExistsAsync(flightId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _service.DeleteAsync(flightId)
        );
        Assert.Equal(flightId, exception.FlightId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw FlightValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task DeleteAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var flightId = 1;
        _mockFlightRepository.Setup(r => r.ExistsAsync(flightId)).ReturnsAsync(true);
        _mockFlightRepository.Setup(r => r.DeleteAsync(flightId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.DeleteAsync(flightId)
        );
        Assert.Contains($"Failed to delete Flight with ID {flightId}", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Valid ID, Flight exists - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_FlightExists_ShouldReturnTrue()
    {
        // Arrange
        var flightId = 1;
        _mockFlightRepository.Setup(r => r.ExistsAsync(flightId)).ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(flightId);

        // Assert
        Assert.True(result);
        _mockFlightRepository.Verify(r => r.ExistsAsync(flightId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Flight does not exist - should return false
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_FlightNotFound_ShouldReturnFalse()
    {
        // Arrange
        var flightId = 999;
        _mockFlightRepository.Setup(r => r.ExistsAsync(flightId)).ReturnsAsync(false);

        // Act
        var result = await _service.ExistsAsync(flightId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw FlightValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task ExistsAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var flightId = 1;
        _mockFlightRepository.Setup(r => r.ExistsAsync(flightId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.ExistsAsync(flightId)
        );
        Assert.Contains($"Failed to check existence of Flight with ID {flightId}", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: No filter, returns correct count - should return count from repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_NoFilter_ShouldReturnCorrectCount()
    {
        // Arrange
        var expectedCount = 42;
        _mockFlightRepository.Setup(r => r.GetCountAsync(null)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(expectedCount, result);
        _mockFlightRepository.Verify(r => r.GetCountAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered count - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new core.filters.FlightFilter { MinPrice = 10000, MaxPrice = 20000 };
        var expectedCount = 5;
        _mockFlightRepository.Setup(r => r.GetCountAsync(filter)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync(filter);

        // Assert
        Assert.Equal(expectedCount, result);
        _mockFlightRepository.Verify(r => r.GetCountAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockFlightRepository.Setup(r => r.GetCountAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to get Flight count", exception.Message);
    }

    #endregion
}
