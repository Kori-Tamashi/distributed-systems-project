using System.Linq.Expressions;
using core.domain;
using core.exceptions.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using dataaccess.repositories.postgres;
using Microsoft.EntityFrameworkCore;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;

using FlightDomain = core.domain.Flight;
using FlightPostgresqlModel = dataaccess.models.postgres.FlightPostgresqlModel;

namespace tests.dataaccess.repositories.postgres.unit;

/// <summary>
/// Unit tests for FlightPostgresqlRepository
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Flight exists in database (normal case)
/// - EP2: Flight does not exist (FlightNotFoundException)
/// - EP3: Database error occurs (FlightDatabaseException)
/// 
/// For CreateAsync(FlightDomain flight):
/// - EP1: Flight created successfully (normal case) - Requires integration test
/// - EP2: Flight with same Id already exists (FlightAlreadyExistsException) - Requires integration test
/// - EP3: Database error occurs (FlightDatabaseException)
/// 
/// For UpdateAsync(FlightDomain flight):
/// - EP1: Flight updated successfully (normal case)
/// - EP2: Flight does not exist (FlightNotFoundException)
/// - EP3: Database error occurs (FlightDatabaseException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Flight deleted successfully (returns true)
/// - EP2: Flight does not exist (returns false)
/// - EP3: Database error occurs (FlightDatabaseException)
/// 
/// Note: Tests using EF Core extension methods (AnyAsync, CountAsync, ToListAsync, etc.)
/// cannot be unit tested with Moq as these methods are not overridable.
/// These should be covered by integration tests with real database.
/// Total: 9 unit tests
/// </summary>
public class FlightPostgresqlRepositoryUnitTests
{
    private readonly Mock<FlightDatabaseContext> _mockContext;
    private readonly Mock<DbSet<FlightPostgresqlModel>> _mockDbSet;
    private readonly FlightPostgresqlRepository _repository;

    public FlightPostgresqlRepositoryUnitTests()
    {
        // Arrange - Setup mock context
        _mockContext = new Mock<FlightDatabaseContext>();
        _mockDbSet = new Mock<DbSet<FlightPostgresqlModel>>();
        
        // Setup DbSet to behave like IQueryable
        var data = new List<FlightPostgresqlModel>().AsQueryable();
        _mockDbSet.As<IQueryable<FlightPostgresqlModel>>()
            .Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<FlightPostgresqlModel>>()
            .Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<FlightPostgresqlModel>>()
            .Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<FlightPostgresqlModel>>()
            .Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        
        _mockContext.Setup(c => c.Flights).Returns(_mockDbSet.Object);
        _repository = new FlightPostgresqlRepository(_mockContext.Object);
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Flight exists in database - should return Flight
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_FlightExists_ShouldReturnFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        var model = new FlightPostgresqlModel
        {
            Id = flight.Id,
            FlightUid = flight.FlightUid,
            FlightNumber = flight.FlightNumber,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = flight.Price
        };

        _mockDbSet.Setup(m => m.FindAsync(flight.Id)).ReturnsAsync(model);

        // Act
        var result = await _repository.GetByIdAsync(flight.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(flight.Id, result.Id);
        Assert.Equal(flight.FlightNumber, result.FlightNumber);
        _mockDbSet.Verify(m => m.FindAsync(flight.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Flight does not exist - should throw FlightNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_FlightNotFound_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var flightId = 999;
        _mockDbSet.Setup(m => m.FindAsync(flightId)).ReturnsAsync((FlightPostgresqlModel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _repository.GetByIdAsync(flightId)
        );
        Assert.Equal(flightId, exception.FlightId);
    }

    /// <summary>
    /// EP3: Database error occurs - should throw FlightDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_DatabaseError_ShouldThrowFlightDatabaseException()
    {
        // Arrange
        var flightId = 1;
        _mockDbSet.Setup(m => m.FindAsync(flightId))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightDatabaseException>(
            () => _repository.GetByIdAsync(flightId)
        );
        Assert.Contains("Failed to get Flight by id", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP3: Database error occurs - should throw FlightDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_DatabaseError_ShouldThrowFlightDatabaseException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = 0;

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightDatabaseException>(
            () => _repository.CreateAsync(flight)
        );
        Assert.Contains("Failed to create Flight", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Flight updated successfully - should return updated Flight
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_Success_ShouldReturnUpdatedFlight()
    {
        // Arrange
        var existingModel = new FlightPostgresqlModel
        {
            Id = 1,
            FlightUid = Guid.NewGuid(),
            FlightNumber = "OLD123",
            DateTime = DateTime.Now,
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 10000
        };

        var updatedFlight = FlightMother.CreateValidFlight();
        updatedFlight.Id = 1;

        _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.UpdateAsync(updatedFlight);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedFlight.FlightNumber, result.FlightNumber);
        Assert.Equal(updatedFlight.Price, result.Price);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        _mockDbSet.Setup(m => m.FindAsync(flight.Id)).ReturnsAsync((FlightPostgresqlModel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _repository.UpdateAsync(flight)
        );
        Assert.Equal(flight.Id, exception.FlightId);
    }

    /// <summary>
    /// EP3: Database error occurs - should throw FlightDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_DatabaseError_ShouldThrowFlightDatabaseException()
    {
        // Arrange
        var existingModel = new FlightPostgresqlModel { Id = 1, FlightNumber = "OLD123" };
        var updatedFlight = FlightMother.CreateValidFlight();
        updatedFlight.Id = 1;

        _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightDatabaseException>(
            () => _repository.UpdateAsync(updatedFlight)
        );
        Assert.Contains("Failed to update Flight", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Flight deleted successfully - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_Success_ShouldReturnTrue()
    {
        // Arrange
        var flightId = 1;
        var existingModel = new FlightPostgresqlModel { Id = flightId, FlightNumber = "SU1234" };

        _mockDbSet.Setup(m => m.FindAsync(flightId)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(flightId);

        // Assert
        Assert.True(result);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// EP2: Flight does not exist - should return false
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_FlightNotFound_ShouldReturnFalse()
    {
        // Arrange
        var flightId = 999;
        _mockDbSet.Setup(m => m.FindAsync(flightId)).ReturnsAsync((FlightPostgresqlModel?)null);

        // Act
        var result = await _repository.DeleteAsync(flightId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Database error occurs - should throw FlightDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_DatabaseError_ShouldThrowFlightDatabaseException()
    {
        // Arrange
        var flightId = 1;
        var existingModel = new FlightPostgresqlModel { Id = flightId, FlightNumber = "SU1234" };

        _mockDbSet.Setup(m => m.FindAsync(flightId)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightDatabaseException>(
            () => _repository.DeleteAsync(flightId)
        );
        Assert.Contains("Failed to delete Flight", exception.Message);
    }

    #endregion
}
