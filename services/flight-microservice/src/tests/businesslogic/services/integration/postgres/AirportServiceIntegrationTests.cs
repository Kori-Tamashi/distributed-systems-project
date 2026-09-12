using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using businesslogic.services;
using core.domain;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.repositories.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.contexts.postgres;
using tests.fixtures.mothers;

using ServiceAirportNotFoundException = core.exceptions.businesslogic.services.AirportNotFoundException;
using ServiceAirportValidationException = core.exceptions.businesslogic.services.AirportValidationException;
using AirportDomain = core.domain.Airport;

namespace tests.businesslogic.services.integration.postgres;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for AirportService
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresqlDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_flights).
/// AirportService is tested with real AirportPostgresqlRepository using TestPostgresqlDatabaseContext.
/// 
/// TEST CONTEXT:
/// - TestPostgresqlDatabaseContext is used for database operations
/// - Database is created once in constructor and cleaned after each test
/// - Each test method cleans up its own data in Dispose
/// - Tests share the same database but are isolated by cleanup
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetByIdAsync Tests:
///    EP1: Valid existing ID (normal case) - airport exists in database
///    EP2: Non-existing ID (error case) - AirportNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - AirportValidationException thrown
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single airport (normal case) - should return list with one airport
///    EP3: Multiple airports (normal case) - should return all airports
///    EP4: Filter by city (normal case) - should return matching airports
///    EP5: Filter by country (normal case) - should return matching airports
///    EP6: Filter with no matches (edge case) - should return empty list
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid airport with all fields (normal case) - should create successfully
///    EP2: Valid airport with minimal data (edge case) - should create successfully
///    EP3: Airport with empty name (validation case) - AirportValidationException thrown
///    EP4: Airport with empty city (validation case) - AirportValidationException thrown
///    EP5: Airport with empty country (validation case) - AirportValidationException thrown
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Non-existing ID (error case) - AirportNotFoundException thrown
///    EP3: Invalid name (validation case) - AirportValidationException thrown
///    EP4: Invalid ID <= 0 (validation case) - AirportValidationException thrown
/// 
/// 5. DeleteAsync Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (error case) - AirportNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - AirportValidationException thrown
///    EP4: Delete and verify data removed (verification case) - GetById throws
/// 
/// 6. ExistsAsync Tests:
///    EP1: Existing ID (normal case) - should return true
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: ID after deletion (verification case) - should return false
///    EP4: Invalid ID <= 0 (validation case) - AirportValidationException thrown
/// 
/// 7. GetCountAsync Tests:
///    EP1: Empty database (edge case) - should return 0
///    EP2: Single airport (normal case) - should return 1
///    EP3: Multiple airports (normal case) - should return correct count
/// 
/// AAA STRUCTURE:
/// All tests follow Arrange-Act-Assert pattern:
/// - Arrange: Setup test data and database state
/// - Act: Execute the method under test
/// - Assert: Verify the results
/// 
/// Total: 30 integration tests
/// </summary>
public class AirportServiceIntegrationTests : IDisposable
{
    private readonly TestPostgresqlDatabaseContext _testContext;
    private readonly IAirportRepository _repository;
    private readonly IAirportService _service;

    public AirportServiceIntegrationTests()
    {
        // Create test database context - this uses TEST_* environment variables
        _testContext = new TestPostgresqlDatabaseContext();
        _testContext.EnsureDatabaseDeleted(); // Create tables automatically
        
        // Create repository and service using the test database context
        _repository = new AirportPostgresqlRepository(_testContext);
        _service = new AirportService(_repository);
    }

    public void Dispose()
    {
        try
        {
            // Clean up all data after each test
            var airports = _testContext.Airports.ToList();
            if (airports.Any())
            {
                _testContext.Airports.RemoveRange(airports);
                _testContext.SaveChanges();
            }
        }
        catch
        {
            // Ignore errors during cleanup (database might not exist)
        }
        finally
        {
            // Dispose test context
            _testContext?.Dispose();
        }
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return airport
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_AirportExists_ShouldReturnAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0;
        var created = await _service.CreateAsync(airport);

        // Act
        var result = await _service.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.Name, result.Name);
        Assert.Equal(created.City, result.City);
        Assert.Equal(created.Country, result.Country);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_NonExistingId_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportNotFoundException>(
            () => _service.GetByIdAsync(nonExistentId)
        );
        Assert.Equal(nonExistentId, exception.AirportId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw AirportValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: Empty database - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange - Database is empty

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// EP2: Single airport - should return list with one airport
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SingleAirport_ShouldReturnListWithOneAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0;
        await _service.CreateAsync(airport);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(airport.Name, result[0].Name);
    }

    /// <summary>
    /// EP3: Multiple airports - should return all airports
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultipleAirports_ShouldReturnAllAirports()
    {
        // Arrange
        var airports = AirportMother.CreateAirportList(5);
        foreach (var airport in airports)
        {
            airport.Id = 0;
            await _service.CreateAsync(airport);
        }

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
    }

    /// <summary>
    /// EP4: Filter by city - should return matching airports
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithCityFilter_ShouldReturnMatchingAirports()
    {
        // Arrange
        var airport1 = new AirportBuilder().WithId(0).WithName("Sheremetyevo").WithCity("Moscow").WithCountry("Russia").Build();
        var airport2 = new AirportBuilder().WithId(0).WithName("Domodedovo").WithCity("Moscow").WithCountry("Russia").Build();
        var airport3 = new AirportBuilder().WithId(0).WithName("Pulkovo").WithCity("Saint Petersburg").WithCountry("Russia").Build();
        
        await _service.CreateAsync(airport1);
        await _service.CreateAsync(airport2);
        await _service.CreateAsync(airport3);

        var filter = new AirportFilter { City = "Moscow" };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal("Moscow", a.City));
    }

    /// <summary>
    /// EP5: Filter by country - should return matching airports
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithCountryFilter_ShouldReturnMatchingAirports()
    {
        // Arrange
        var airport1 = new AirportBuilder().WithId(0).WithName("Sheremetyevo").WithCity("Moscow").WithCountry("Russia").Build();
        var airport2 = new AirportBuilder().WithId(0).WithName("Pulkovo").WithCity("Saint Petersburg").WithCountry("Russia").Build();
        var airport3 = new AirportBuilder().WithId(0).WithName("Charles de Gaulle").WithCity("Paris").WithCountry("France").Build();
        
        await _service.CreateAsync(airport1);
        await _service.CreateAsync(airport2);
        await _service.CreateAsync(airport3);

        var filter = new AirportFilter { Country = "Russia" };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal("Russia", a.Country));
    }

    /// <summary>
    /// EP6: Filter with no matches - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilterNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0;
        await _service.CreateAsync(airport);

        var filter = new AirportFilter { City = "NonExistent" };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid airport with all fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidAirportWithAllFields_ShouldCreateAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0;

        // Act
        var result = await _service.CreateAsync(airport);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(airport.Name, result.Name);
        Assert.Equal(airport.City, result.City);
        Assert.Equal(airport.Country, result.Country);
        
        // Verify in database
        var retrieved = await _service.GetByIdAsync(result.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(result.Id, retrieved.Id);
    }

    /// <summary>
    /// EP2: Valid airport with minimal data - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidAirportWithMinimalData_ShouldCreateAirport()
    {
        // Arrange
        var airport = new AirportBuilder()
            .WithId(0)
            .WithName("Airport")
            .WithCity("City")
            .WithCountry("Country")
            .Build();

        // Act
        var result = await _service.CreateAsync(airport);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(airport.Name, result.Name);
    }

    /// <summary>
    /// EP3: Airport with empty name - should throw AirportValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_EmptyName_ShouldThrowAirportValidationException()
    {
        // Arrange
        var airport = AirportMother.CreateAirportWithEmptyName();
        airport.Id = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportValidationException>(
            () => _service.CreateAsync(airport)
        );
        Assert.Contains("Airport validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Airport with empty city - should throw AirportValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_EmptyCity_ShouldThrowAirportValidationException()
    {
        // Arrange
        var airport = new Airport
        {
            Name = "Test Airport",
            City = string.Empty,
            Country = "Russia"
        };
        airport.Id = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportValidationException>(
            () => _service.CreateAsync(airport)
        );
        Assert.Contains("Airport validation failed", exception.Message);
    }

    /// <summary>
    /// EP5: Airport with empty country - should throw AirportValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_EmptyCountry_ShouldThrowAirportValidationException()
    {
        // Arrange
        var airport = new Airport
        {
            Name = "Test Airport",
            City = "Moscow",
            Country = string.Empty
        };
        airport.Id = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportValidationException>(
            () => _service.CreateAsync(airport)
        );
        Assert.Contains("Airport validation failed", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid update with all fields - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_ValidUpdate_ShouldUpdateAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0;
        var created = await _service.CreateAsync(airport);
        
        var updatedAirport = new Airport
        {
            Id = created.Id,
            Name = "Updated Airport Name",
            City = "Updated City",
            Country = "Updated Country"
        };

        // Act
        var result = await _service.UpdateAsync(updatedAirport);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Airport Name", result.Name);
        Assert.Equal("Updated City", result.City);
        Assert.Equal("Updated Country", result.Country);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportNotFoundException>(
            () => _service.UpdateAsync(airport)
        );
        Assert.Equal(airport.Id, exception.AirportId);
    }

    /// <summary>
    /// EP3: Invalid name - should throw AirportValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidName_ShouldThrowAirportValidationException()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0;
        var created = await _service.CreateAsync(airport);
        
        var invalidAirport = new Airport
        {
            Id = created.Id,
            Name = string.Empty, // Invalid
            City = "Moscow",
            Country = "Russia"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportValidationException>(
            () => _service.UpdateAsync(invalidAirport)
        );
        Assert.Contains("Airport validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw AirportValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task UpdateAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportValidationException>(
            () => _service.UpdateAsync(airport)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return true and delete
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_AirportExists_ShouldReturnTrueAndDelete()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0;
        var created = await _service.CreateAsync(airport);

        // Act
        var result = await _service.DeleteAsync(created.Id);

        // Assert
        Assert.True(result);
        
        // Verify airport is deleted
        var exception = await Assert.ThrowsAsync<ServiceAirportNotFoundException>(
            () => _service.GetByIdAsync(created.Id)
        );
        Assert.Equal(created.Id, exception.AirportId);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportNotFoundException>(
            () => _service.DeleteAsync(nonExistentId)
        );
        Assert.Equal(nonExistentId, exception.AirportId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw AirportValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    /// <summary>
    /// EP4: Delete and verify data removed - GetById should throw
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_VerifyDataRemoved_ShouldThrowNotFoundException()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0;
        var created = await _service.CreateAsync(airport);

        // Act
        await _service.DeleteAsync(created.Id);

        // Assert - Verify data is completely removed
        var exception = await Assert.ThrowsAsync<ServiceAirportNotFoundException>(
            () => _service.GetByIdAsync(created.Id)
        );
        Assert.Equal(created.Id, exception.AirportId);
        
        // Also verify ExistsAsync returns false
        var exists = await _service.ExistsAsync(created.Id);
        Assert.False(exists);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Existing ID - should return true
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_AirportExists_ShouldReturnTrue()
    {
        // Arrange
        var airport = new AirportBuilder().WithId(0).WithName("Test Airport").WithCity("Test City").WithCountry("Test Country").Build();
        var created = await _service.CreateAsync(airport);

        // Act
        var result = await _service.ExistsAsync(created.Id);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_AirportNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var result = await _service.ExistsAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: ID after deletion - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_AfterDeletion_ShouldReturnFalse()
    {
        // Arrange
        var airport = new AirportBuilder().WithId(0).WithName("Test Airport").WithCity("Test City").WithCountry("Test Country").Build();
        var created = await _service.CreateAsync(airport);
        await _service.DeleteAsync(created.Id);

        // Act
        var result = await _service.ExistsAsync(created.Id);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw AirportValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task ExistsAsync_InvalidId_ShouldThrowAirportValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Airport ID", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: Empty database - should return 0
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_EmptyDatabase_ShouldReturnZero()
    {
        // Arrange - Database is empty

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// EP2: Single airport - should return 1
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_SingleAirport_ShouldReturnOne()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        airport.Id = 0;
        await _service.CreateAsync(airport);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(1, result);
    }

    /// <summary>
    /// EP3: Multiple airports - should return correct count
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_MultipleAirports_ShouldReturnCorrectCount()
    {
        // Arrange
        var airports = AirportMother.CreateAirportList(7);
        foreach (var airport in airports)
        {
            airport.Id = 0;
            await _service.CreateAsync(airport);
        }

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(7, result);
    }

    #endregion
}
