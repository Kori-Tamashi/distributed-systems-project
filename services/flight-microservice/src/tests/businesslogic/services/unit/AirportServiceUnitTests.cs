using core.domain;
using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using businesslogic.services;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;

using RepositoryAirportAlreadyExistsException = core.exceptions.dataaccess.repositories.AirportAlreadyExistsException;
using RepositoryAirportNotFoundException = core.exceptions.dataaccess.repositories.AirportNotFoundException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for AirportService
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, Airport exists (normal case)
/// - EP2: Valid ID, Airport does not exist (AirportNotFoundException)
/// - EP3: Invalid ID (<= 0) (AirportValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetAllAsync(AirportFilter? filter):
/// - EP1: No filter, returns all airports
/// - EP2: With filter, returns filtered airports
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// For CreateAsync(Airport airport):
/// - EP1: Valid airport, creation successful (normal case)
/// - EP2: Invalid airport (AirportValidationException)
/// - EP3: Airport already exists (AirportBusinessRuleViolationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For UpdateAsync(Airport airport):
/// - EP1: Valid airport, Airport exists, update successful (normal case)
/// - EP2: Airport does not exist (AirportNotFoundException)
/// - EP3: Invalid airport (AirportValidationException)
/// - EP4: Invalid ID (AirportValidationException)
/// - EP5: Repository throws exception (BaseServiceException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, Airport exists, deletion successful (normal case)
/// - EP2: Valid ID, Airport does not exist (AirportNotFoundException)
/// - EP3: Invalid ID (<= 0) (AirportValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, Airport exists (returns true)
/// - EP2: Valid ID, Airport does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (AirportValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetCountAsync(AirportFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// Total: 23 unit tests (all should pass)
/// </summary>
public class AirportServiceUnitTests
{
    private readonly Mock<IAirportRepository> _mockRepository;
    private readonly IAirportService _service;

    public AirportServiceUnitTests()
    {
        // Arrange - Setup mock repository
        _mockRepository = new Mock<IAirportRepository>();
        _service = new AirportService(_mockRepository.Object);
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid ID, Airport exists - should return Airport
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_AirportExists_ShouldReturnAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        _mockRepository.Setup(r => r.GetByIdAsync(airport.Id)).ReturnsAsync(airport);

        // Act
        var result = await _service.GetByIdAsync(airport.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(airport.Id, result.Id);
        Assert.Equal(airport.Name, result.Name);
        _mockRepository.Verify(r => r.GetByIdAsync(airport.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Airport does not exist - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var airportId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(airportId)).ThrowsAsync(new RepositoryAirportNotFoundException(airportId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _service.GetByIdAsync(airportId)
        );
        Assert.Equal(airportId, exception.AirportId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw AirportValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Unit]
    public async Task GetByIdAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var airportId = 1;
        _mockRepository.Setup(r => r.GetByIdAsync(airportId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetByIdAsync(airportId)
        );
        Assert.Contains($"Failed to get Airport with ID {airportId}", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: No filter, returns all airports - should return list of airports
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllAirports()
    {
        // Arrange
        var airports = AirportMother.CreateAirportList(5);
        _mockRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(airports);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        _mockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered airports - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new core.filters.AirportFilter { City = "Moscow" };
        var filteredAirports = new List<Airport> { AirportMother.CreateMoscowSheremetyevo() };
        _mockRepository.Setup(r => r.GetAllAsync(filter)).ReturnsAsync(filteredAirports);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockRepository.Verify(r => r.GetAllAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to get all Airports", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid airport, creation successful - should return created airport
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_ValidAirport_ShouldReturnCreatedAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0; // Id will be generated
        _mockRepository.Setup(r => r.CreateAsync(airport)).ReturnsAsync(airport);

        // Act
        var result = await _service.CreateAsync(airport);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.CreateAsync(airport), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid airport (empty name) - should throw AirportValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_InvalidAirport_EmptyName_ShouldThrowAirportValidationException()
    {
        // Arrange
        var airport = AirportMother.CreateAirportWithEmptyName();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.CreateAsync(airport)
        );
        Assert.Contains("Airport validation failed", exception.Message);
    }

    /// <summary>
    /// EP3: Invalid airport (empty city) - should throw AirportValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_InvalidAirport_EmptyCity_ShouldThrowAirportValidationException()
    {
        // Arrange
        var airport = new Airport
        {
            Name = "Test Airport",
            City = string.Empty,
            Country = "Russia"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.CreateAsync(airport)
        );
        Assert.Contains("Airport validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid airport (empty country) - should throw AirportValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_InvalidAirport_EmptyCountry_ShouldThrowAirportValidationException()
    {
        // Arrange
        var airport = new Airport
        {
            Name = "Test Airport",
            City = "Moscow",
            Country = string.Empty
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.CreateAsync(airport)
        );
        Assert.Contains("Airport validation failed", exception.Message);
    }

    /// <summary>
    /// EP5: Airport already exists - should throw AirportBusinessRuleViolationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_AirportAlreadyExists_ShouldThrowAirportBusinessRuleViolationException()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        _mockRepository.Setup(r => r.CreateAsync(airport))
            .ThrowsAsync(new RepositoryAirportAlreadyExistsException(airport.Id));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportBusinessRuleViolationException>(
            () => _service.CreateAsync(airport)
        );
        Assert.Contains("UniqueConstraint", exception.RuleName);
        Assert.Contains("already exists", exception.Message);
    }

    /// <summary>
    /// EP6: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0;
        _mockRepository.Setup(r => r.CreateAsync(airport))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.CreateAsync(airport)
        );
        Assert.Contains("Failed to create Airport", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid airport, Airport exists, update successful - should return updated airport
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_ValidAirport_AirportExists_ShouldReturnUpdatedAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        _mockRepository.Setup(r => r.ExistsAsync(airport.Id)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.UpdateAsync(airport)).ReturnsAsync(airport);

        // Act
        var result = await _service.UpdateAsync(airport);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.ExistsAsync(airport.Id), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(airport), Times.Once);
    }

    /// <summary>
    /// EP2: Airport does not exist - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        _mockRepository.Setup(r => r.ExistsAsync(airport.Id)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _service.UpdateAsync(airport)
        );
        Assert.Equal(airport.Id, exception.AirportId);
    }

    /// <summary>
    /// EP3: Invalid airport (empty name) - should throw AirportValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_InvalidAirport_EmptyName_ShouldThrowAirportValidationException()
    {
        // Arrange
        var airport = AirportMother.CreateAirportWithEmptyName();
        _mockRepository.Setup(r => r.ExistsAsync(airport.Id)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.UpdateAsync(airport)
        );
        Assert.Contains("Airport validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID (<= 0) - should throw AirportValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task UpdateAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.UpdateAsync(airport)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    /// <summary>
    /// EP5: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        _mockRepository.Setup(r => r.ExistsAsync(airport.Id)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.UpdateAsync(airport))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.UpdateAsync(airport)
        );
        Assert.Contains($"Failed to update Airport with ID {airport.Id}", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid ID, Airport exists, deletion successful - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_AirportExists_ShouldReturnTrue()
    {
        // Arrange
        var airportId = 1;
        _mockRepository.Setup(r => r.ExistsAsync(airportId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(airportId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(airportId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.ExistsAsync(airportId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(airportId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Airport does not exist - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var airportId = 999;
        _mockRepository.Setup(r => r.ExistsAsync(airportId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _service.DeleteAsync(airportId)
        );
        Assert.Equal(airportId, exception.AirportId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw AirportValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task DeleteAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var airportId = 1;
        _mockRepository.Setup(r => r.ExistsAsync(airportId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(airportId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.DeleteAsync(airportId)
        );
        Assert.Contains($"Failed to delete Airport with ID {airportId}", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Valid ID, Airport exists - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_AirportExists_ShouldReturnTrue()
    {
        // Arrange
        var airportId = 1;
        _mockRepository.Setup(r => r.ExistsAsync(airportId)).ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(airportId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.ExistsAsync(airportId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Airport does not exist - should return false
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_AirportNotFound_ShouldReturnFalse()
    {
        // Arrange
        var airportId = 999;
        _mockRepository.Setup(r => r.ExistsAsync(airportId)).ReturnsAsync(false);

        // Act
        var result = await _service.ExistsAsync(airportId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw AirportValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task ExistsAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var airportId = 1;
        _mockRepository.Setup(r => r.ExistsAsync(airportId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.ExistsAsync(airportId)
        );
        Assert.Contains($"Failed to check existence of Airport with ID {airportId}", exception.Message);
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
        _mockRepository.Setup(r => r.GetCountAsync(null)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(expectedCount, result);
        _mockRepository.Verify(r => r.GetCountAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered count - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new core.filters.AirportFilter { Country = "Russia" };
        var expectedCount = 10;
        _mockRepository.Setup(r => r.GetCountAsync(filter)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync(filter);

        // Assert
        Assert.Equal(expectedCount, result);
        _mockRepository.Verify(r => r.GetCountAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetCountAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to get Airport count", exception.Message);
    }

    #endregion
}
