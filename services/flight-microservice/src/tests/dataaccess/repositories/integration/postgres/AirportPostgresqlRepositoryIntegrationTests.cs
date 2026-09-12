using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;
using core.interfaces.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.repositories.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.contexts.postgres;
using tests.fixtures.mothers;

using AirportDomain = core.domain.Airport;

namespace tests.dataaccess.repositories.unit.postgres;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for AirportPostgresqlRepository
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresqlDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_flights).
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetByIdAsync Tests:
///    EP1: Valid existing ID (normal case) - airport exists in database
///    EP2: Non-existing ID (edge case) - airport not found, should throw AirportNotFoundException
///    EP3: Airport with minimal data (edge case) - airport with just code
///    EP4: Airport with full data (normal case) - airport with all fields
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single airport (normal case) - should return list with one airport
///    EP3: Multiple airports (normal case) - should return all airports
///    EP4: Filter by code (normal case) - should return matching airports
///    EP5: Filter by city (normal case) - should return airports in city
///    EP6: Filter with no matches (edge case) - should return empty list
///    EP7: Null filter (normal case) - should return all airports
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid airport with all fields (normal case) - should create successfully
///    EP2: Valid airport with minimal fields (edge case) - should create successfully
///    EP3: Duplicate ID (error case) - should throw AirportAlreadyExistsException
///    EP4: Airport with max name length (boundary case) - should create successfully
///    EP5: Airport with empty name (edge case) - should create successfully
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Update with minimal fields (edge case) - should update successfully
///    EP3: Non-existing ID (error case) - should throw AirportNotFoundException
///    EP4: Update name to max length (boundary case) - should update successfully
/// 
/// 5. DeleteAsync Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: Delete and verify data removed (verification case) - data should be gone
/// 
/// 6. ExistsAsync Tests:
///    EP1: Existing ID (normal case) - should return true
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: ID after deletion (verification case) - should return false
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
/// </summary>
public class AirportPostgresqlRepositoryIntegrationTests : IDisposable
{
    private readonly TestPostgresqlDatabaseContext _context;
    private readonly IAirportRepository _repository;
    private readonly List<AirportDomain> _createdAirports;

    public AirportPostgresqlRepositoryIntegrationTests()
    {
        // Create test database context
        _context = new TestPostgresqlDatabaseContext();
        _context.EnsureDatabaseDeleted();
        
        // Create repository using the test database connection
        _repository = new AirportPostgresqlRepository(_context);
        
        // Track created airports for cleanup
        _createdAirports = new List<AirportDomain>();
    }

    #region IDisposable Implementation

    public void Dispose()
    {
        try
        {
            // Clean up all data
            var airports = _context.Airports.ToList();
            if (airports.Any())
            {
                _context.Airports.RemoveRange(airports);
                _context.SaveChanges();
            }
        }
        catch
        {
            // Ignore errors during cleanup (database might not exist)
        }
        finally
        {
            _context?.Dispose();
        }
    }

    #endregion

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - airport exists in database
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_AirportExists_ShouldReturnAirport()
    {
        // Arrange
        var expectedAirport = AirportMother.CreateValidAirport();
        await _repository.CreateAsync(expectedAirport);
        _createdAirports.Add(expectedAirport);

        // Act
        var actualAirport = await _repository.GetByIdAsync(expectedAirport.Id);

        // Assert
        Assert.NotNull(actualAirport);
        Assert.Equal(expectedAirport.Id, actualAirport.Id);
        Assert.Equal(expectedAirport.Name, actualAirport.Name);
        Assert.Equal(expectedAirport.Name, actualAirport.Name);
        Assert.Equal(expectedAirport.City, actualAirport.City);
        Assert.Equal(expectedAirport.Country, actualAirport.Country);
    }

    /// <summary>
    /// EP2: Non-existing ID - airport not found
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var nonExistingId = 999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _repository.GetByIdAsync(nonExistingId));
        
        Assert.Equal(nonExistingId, exception.AirportId);
    }

    /// <summary>
    /// EP3: Airport with minimal data - just code
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_MinimalAirport_ShouldReturnAirport()
    {
        // Arrange
        var expectedAirport = AirportMother.CreateMinimalAirport();
        await _repository.CreateAsync(expectedAirport);
        _createdAirports.Add(expectedAirport);

        // Act
        var actualAirport = await _repository.GetByIdAsync(expectedAirport.Id);

        // Assert
        Assert.NotNull(actualAirport);
        Assert.Equal(expectedAirport.Id, actualAirport.Id);
        Assert.Equal(expectedAirport.Name, actualAirport.Name);
        Assert.Equal(string.Empty, actualAirport.City);
        Assert.Equal(string.Empty, actualAirport.Country);
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
        // Arrange (database is already empty)

        // Act
        var airports = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(airports);
        Assert.Empty(airports);
    }

    /// <summary>
    /// EP2: Single airport - should return list with one airport
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SingleAirport_ShouldReturnListWithOneAirport()
    {
        // Arrange
        var expectedAirport = AirportMother.CreateValidAirport();
        await _repository.CreateAsync(expectedAirport);
        _createdAirports.Add(expectedAirport);

        // Act
        var airports = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(airports);
        Assert.Single(airports);
        Assert.Equal(expectedAirport.Id, airports[0].Id);
    }

    /// <summary>
    /// EP3: Multiple airports - should return all airports
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultipleAirports_ShouldReturnAllAirports()
    {
        // Arrange
        var expectedAirports = AirportMother.CreateAirportList(5);
        foreach (var airport in expectedAirports)
        {
            await _repository.CreateAsync(airport);
            _createdAirports.Add(airport);
        }

        // Act
        var airports = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(airports);
        Assert.Equal(5, airports.Count);
    }

    /// <summary>
    /// EP4: Filter by name - should return matching airports
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithNameFilter_ShouldReturnMatchingAirports()
    {
        // Arrange
        var svoAirport = AirportMother.CreateMoscowSheremetyevo();   // Sheremetyevo
        var dmeAirport = AirportMother.CreateMoscowDomodedovo();     // Domodedovo
        var pwkAirport = AirportMother.CreatePetersburgPulkovo();    // Pulkovo
        
        await _repository.CreateAsync(svoAirport);
        await _repository.CreateAsync(dmeAirport);
        await _repository.CreateAsync(pwkAirport);
        _createdAirports.AddRange(new[] { svoAirport, dmeAirport, pwkAirport });

        // Act
        var airports = await _repository.GetAllAsync(new AirportFilter { Name = "Sheremetyevo International Airport" });

        // Assert
        Assert.NotNull(airports);
        Assert.Single(airports);
        Assert.Equal("Sheremetyevo International Airport", airports[0].Name);
    }

    /// <summary>
    /// EP5: Filter by city - should return airports in city
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithCityFilter_ShouldReturnAirportsInCity()
    {
        // Arrange
        var svoAirport = AirportMother.CreateMoscowSheremetyevo();   // Moscow
        var dmeAirport = AirportMother.CreateMoscowDomodedovo();     // Moscow
        var pwkAirport = AirportMother.CreatePetersburgPulkovo();    // Petersburg
        
        await _repository.CreateAsync(svoAirport);
        await _repository.CreateAsync(dmeAirport);
        await _repository.CreateAsync(pwkAirport);
        _createdAirports.AddRange(new[] { svoAirport, dmeAirport, pwkAirport });

        // Act
        var airports = await _repository.GetAllAsync(new AirportFilter { City = "Moscow" });

        // Assert
        Assert.NotNull(airports);
        Assert.Equal(2, airports.Count);
        Assert.All(airports, a => Assert.Equal("Moscow", a.City));
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
        await _repository.CreateAsync(airport);
        _createdAirports.Add(airport);

        // Act
        var airports = await _repository.GetAllAsync(new AirportFilter { Name = "NONEXISTENT" });

        // Assert
        Assert.NotNull(airports);
        Assert.Empty(airports);
    }

    /// <summary>
    /// EP7: Null filter - should return all airports
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithNullFilter_ShouldReturnAllAirports()
    {
        // Arrange
        var airports = AirportMother.CreateAirportList(3);
        foreach (var airport in airports)
        {
            await _repository.CreateAsync(airport);
            _createdAirports.Add(airport);
        }

        // Act
        var result = await _repository.GetAllAsync(filter: null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid airport with all fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidAirport_ShouldCreateAirport()
    {
        // Arrange
        var airportToCreate = AirportMother.CreateValidAirport();

        // Act
        var createdAirport = await _repository.CreateAsync(airportToCreate);
        _createdAirports.Add(createdAirport);

        // Assert
        Assert.NotNull(createdAirport);
        Assert.NotEqual(0, createdAirport.Id);
        Assert.Equal(airportToCreate.Name, createdAirport.Name);
        Assert.Equal(airportToCreate.Name, createdAirport.Name);
        Assert.Equal(airportToCreate.City, createdAirport.City);
        Assert.Equal(airportToCreate.Country, createdAirport.Country);
    }

    /// <summary>
    /// EP2: Valid airport with minimal fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_MinimalAirport_ShouldCreateAirport()
    {
        // Arrange
        var airportToCreate = AirportMother.CreateMinimalAirport();

        // Act
        var createdAirport = await _repository.CreateAsync(airportToCreate);
        _createdAirports.Add(createdAirport);

        // Assert
        Assert.NotNull(createdAirport);
        Assert.NotEqual(0, createdAirport.Id);
        Assert.Equal(airportToCreate.Name, createdAirport.Name);
        Assert.Equal(string.Empty, createdAirport.City);
        Assert.Equal(string.Empty, createdAirport.Country);
    }

    /// <summary>
    /// EP3: Duplicate ID - should throw AirportAlreadyExistsException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_DuplicateId_ShouldThrowAirportAlreadyExistsException()
    {
        // Arrange
        var airport1 = new AirportBuilder().WithId(1).WithName("AIR1").Build();
        var airport2 = new AirportBuilder().WithId(1).WithName("AIR2").Build();
        
        await _repository.CreateAsync(airport1);
        _createdAirports.Add(airport1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportAlreadyExistsException>(
            () => _repository.CreateAsync(airport2));
        
        Assert.Equal(1, exception.AirportId);
    }

    /// <summary>
    /// EP4: Airport with max name length - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_AirportWithMaxName_ShouldCreateAirport()
    {
        // Arrange
        var airportToCreate = AirportMother.CreateAirportWithMaxName();

        // Act
        var createdAirport = await _repository.CreateAsync(airportToCreate);
        _createdAirports.Add(createdAirport);

        // Assert
        Assert.NotNull(createdAirport);
        Assert.Equal(airportToCreate.Name, createdAirport.Name);
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
        await _repository.CreateAsync(airport);
        _createdAirports.Add(airport);

        airport.Name = "Updated Airport Name";
        airport.City = "Updated City";
        airport.Country = "Updated Country";

        // Act
        var updatedAirport = await _repository.UpdateAsync(airport);

        // Assert
        Assert.NotNull(updatedAirport);
        Assert.Equal("Updated Airport Name", updatedAirport.Name);
        Assert.Equal("Updated City", updatedAirport.City);
        Assert.Equal("Updated Country", updatedAirport.Country);
    }

    /// <summary>
    /// EP2: Update to minimal fields - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_UpdateToMinimalFields_ShouldUpdateAirport()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        await _repository.CreateAsync(airport);
        _createdAirports.Add(airport);

        airport.City = string.Empty;
        airport.Country = string.Empty;

        // Act
        var updatedAirport = await _repository.UpdateAsync(airport);

        // Assert
        Assert.NotNull(updatedAirport);
        Assert.Equal(string.Empty, updatedAirport.City);
        Assert.Equal(string.Empty, updatedAirport.Country);
    }

    /// <summary>
    /// EP3: Non-existing ID - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var airport = new AirportBuilder().WithId(999).WithName("NonExistent").Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _repository.UpdateAsync(airport));
        
        Assert.Equal(999, exception.AirportId);
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
        await _repository.CreateAsync(airport);
        _createdAirports.Add(airport);

        // Act
        var result = await _repository.DeleteAsync(airport.Id);

        // Assert
        Assert.True(result);
        
        // Verify airport is deleted
        var exists = await _repository.ExistsAsync(airport.Id);
        Assert.False(exists);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_AirportNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var result = await _repository.DeleteAsync(nonExistingId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Delete and verify data removed - should throw NotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_VerifyDataRemoved_ShouldThrowNotFoundException()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        await _repository.CreateAsync(airport);
        _createdAirports.Add(airport);

        // Act
        await _repository.DeleteAsync(airport.Id);

        // Assert
        await Assert.ThrowsAsync<AirportNotFoundException>(
            () => _repository.GetByIdAsync(airport.Id));
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
        var airport = AirportMother.CreateValidAirport();
        await _repository.CreateAsync(airport);
        _createdAirports.Add(airport);

        // Act
        var exists = await _repository.ExistsAsync(airport.Id);

        // Assert
        Assert.True(exists);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_AirportNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var exists = await _repository.ExistsAsync(nonExistingId);

        // Assert
        Assert.False(exists);
    }

    /// <summary>
    /// EP3: ID after deletion - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_AfterDeletion_ShouldReturnFalse()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        await _repository.CreateAsync(airport);
        _createdAirports.Add(airport);
        
        await _repository.DeleteAsync(airport.Id);

        // Act
        var exists = await _repository.ExistsAsync(airport.Id);

        // Assert
        Assert.False(exists);
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
        // Arrange (database is already empty)

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(0, count);
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
        await _repository.CreateAsync(airport);
        _createdAirports.Add(airport);

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(1, count);
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
            await _repository.CreateAsync(airport);
            _createdAirports.Add(airport);
        }

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(7, count);
    }

    #endregion
}
