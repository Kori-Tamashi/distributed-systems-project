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

using AirportDomain = core.domain.Airport;
using AirportPostgresqlModel = dataaccess.models.postgres.AirportPostgresqlModel;

namespace tests.dataaccess.repositories.postgres.unit;

/// <summary>
/// Unit tests for AirportPostgresqlRepository
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Airport exists in database (normal case)
/// - EP2: Airport does not exist (AirportNotFoundException)
/// - EP3: Database error occurs (AirportDatabaseException)
/// 
/// For CreateAsync(AirportDomain airport):
/// - EP1: Airport created successfully (normal case) - Requires integration test
/// - EP2: Airport with same Id already exists (AirportAlreadyExistsException) - Requires integration test
/// - EP3: Database error occurs (AirportDatabaseException)
/// 
/// For UpdateAsync(AirportDomain airport):
/// - EP1: Airport updated successfully (normal case)
/// - EP2: Airport does not exist (AirportNotFoundException)
/// - EP3: Database error occurs (AirportDatabaseException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Airport deleted successfully (returns true)
/// - EP2: Airport does not exist (returns false)
/// - EP3: Database error occurs (AirportDatabaseException)
/// 
/// Note: Tests using EF Core extension methods (AnyAsync, CountAsync, ToListAsync, etc.)
/// cannot be unit tested with Moq as these methods are not overridable.
/// These should be covered by integration tests with real database.
/// Total: 9 unit tests
/// </summary>
public class AirportPostgresqlRepositoryUnitTests
{
    private readonly Mock<FlightDatabaseContext> _mockContext;
    private readonly Mock<DbSet<AirportPostgresqlModel>> _mockDbSet;
    private readonly AirportPostgresqlRepository _repository;

    public AirportPostgresqlRepositoryUnitTests()
    {
        // Arrange - Setup mock context
        _mockContext = new Mock<FlightDatabaseContext>();
        _mockDbSet = new Mock<DbSet<AirportPostgresqlModel>>();
        
        // Setup DbSet to behave like IQueryable
        var data = new List<AirportPostgresqlModel>().AsQueryable();
        _mockDbSet.As<IQueryable<AirportPostgresqlModel>>()
            .Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<AirportPostgresqlModel>>()
            .Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<AirportPostgresqlModel>>()
            .Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<AirportPostgresqlModel>>()
            .Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        
        _mockContext.Setup(c => c.Airports).Returns(_mockDbSet.Object);
        _repository = new AirportPostgresqlRepository(_mockContext.Object);
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Airport exists in database - should return Airport
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_AirportExists_ShouldReturnAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        var model = new AirportPostgresqlModel
        {
            Id = airport.Id,
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country
        };

        _mockDbSet.Setup(m => m.FindAsync(airport.Id)).ReturnsAsync(model);

        // Act
        var result = await _repository.GetByIdAsync(airport.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(airport.Id, result.Id);
        Assert.Equal(airport.Name, result.Name);
        _mockDbSet.Verify(m => m.FindAsync(airport.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Airport does not exist - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var airportId = 999;
        _mockDbSet.Setup(m => m.FindAsync(airportId)).ReturnsAsync((AirportPostgresqlModel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _repository.GetByIdAsync(airportId)
        );
        Assert.Equal(airportId, exception.AirportId);
    }

    /// <summary>
    /// EP3: Database error occurs - should throw AirportDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_DatabaseError_ShouldThrowAirportDatabaseException()
    {
        // Arrange
        var airportId = 1;
        _mockDbSet.Setup(m => m.FindAsync(airportId))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportDatabaseException>(
            () => _repository.GetByIdAsync(airportId)
        );
        Assert.Contains("Failed to get Airport by id", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP3: Database error occurs - should throw AirportDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_DatabaseError_ShouldThrowAirportDatabaseException()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0;

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportDatabaseException>(
            () => _repository.CreateAsync(airport)
        );
        Assert.Contains("Failed to create Airport", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Airport updated successfully - should return updated Airport
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_Success_ShouldReturnUpdatedAirport()
    {
        // Arrange
        var existingModel = new AirportPostgresqlModel
        {
            Id = 1,
            Name = "Old Airport Name",
            City = "Old City",
            Country = "Old Country"
        };

        var updatedAirport = AirportMother.CreateValidAirport();
        updatedAirport.Id = 1;

        _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.UpdateAsync(updatedAirport);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedAirport.Name, result.Name);
        Assert.Equal(updatedAirport.City, result.City);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        _mockDbSet.Setup(m => m.FindAsync(airport.Id)).ReturnsAsync((AirportPostgresqlModel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _repository.UpdateAsync(airport)
        );
        Assert.Equal(airport.Id, exception.AirportId);
    }

    /// <summary>
    /// EP3: Database error occurs - should throw AirportDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_DatabaseError_ShouldThrowAirportDatabaseException()
    {
        // Arrange
        var existingModel = new AirportPostgresqlModel { Id = 1, Name = "Old Name" };
        var updatedAirport = AirportMother.CreateValidAirport();
        updatedAirport.Id = 1;

        _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportDatabaseException>(
            () => _repository.UpdateAsync(updatedAirport)
        );
        Assert.Contains("Failed to update Airport", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Airport deleted successfully - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_Success_ShouldReturnTrue()
    {
        // Arrange
        var airportId = 1;
        var existingModel = new AirportPostgresqlModel { Id = airportId, Name = "Test Airport" };

        _mockDbSet.Setup(m => m.FindAsync(airportId)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(airportId);

        // Assert
        Assert.True(result);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// EP2: Airport does not exist - should return false
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_AirportNotFound_ShouldReturnFalse()
    {
        // Arrange
        var airportId = 999;
        _mockDbSet.Setup(m => m.FindAsync(airportId)).ReturnsAsync((AirportPostgresqlModel?)null);

        // Act
        var result = await _repository.DeleteAsync(airportId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Database error occurs - should throw AirportDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_DatabaseError_ShouldThrowAirportDatabaseException()
    {
        // Arrange
        var airportId = 1;
        var existingModel = new AirportPostgresqlModel { Id = airportId, Name = "Test Airport" };

        _mockDbSet.Setup(m => m.FindAsync(airportId)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportDatabaseException>(
            () => _repository.DeleteAsync(airportId)
        );
        Assert.Contains("Failed to delete Airport", exception.Message);
    }

    #endregion
}
