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

using FlightDomain = core.domain.Flight;

namespace tests.dataaccess.repositories.unit.postgres;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for FlightPostgresqlRepository
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresqlDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_flights).
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetByIdAsync Tests:
///    EP1: Valid existing ID (normal case) - flight exists in database
///    EP2: Non-existing ID (edge case) - flight not found, should throw FlightNotFoundException
///    EP3: Flight with null optional fields (edge case) - flight with minimal data
///    EP4: Flight with all fields populated (normal case) - flight with full data
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single flight (normal case) - should return list with one flight
///    EP3: Multiple flights (normal case) - should return all flights
///    EP4: Filter by flight number (normal case) - should return matching flights
///    EP5: Filter by date range (normal case) - should return flights in range
///    EP6: Filter with no matches (edge case) - should return empty list
///    EP7: Null filter (normal case) - should return all flights
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid flight with all fields (normal case) - should create successfully
///    EP2: Valid flight with null optional fields (edge case) - should create successfully
///    EP3: Flight with zero price (boundary case) - should create successfully
///    EP4: Duplicate ID (error case) - should throw FlightAlreadyExistsException
///    EP5: Flight with max price value (boundary case) - should create successfully
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Update with null optional fields (edge case) - should update successfully
///    EP3: Non-existing ID (error case) - should throw FlightNotFoundException
///    EP4: Update price to boundary values (boundary case) - should update successfully
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
///    EP2: Single flight (normal case) - should return 1
///    EP3: Multiple flights (normal case) - should return correct count
/// 
/// AAA STRUCTURE:
/// All tests follow Arrange-Act-Assert pattern:
/// - Arrange: Setup test data and database state
/// - Act: Execute the method under test
/// - Assert: Verify the results
/// </summary>
public class FlightPostgresqlRepositoryIntegrationTests : IDisposable
{
    private readonly TestPostgresqlDatabaseContext _context;
    private readonly IFlightRepository _repository;
    private readonly IAirportRepository _airportRepository;
    private readonly List<FlightDomain> _createdFlights;

    public FlightPostgresqlRepositoryIntegrationTests()
    {
        // Create test database context
        _context = new TestPostgresqlDatabaseContext();
        _context.EnsureDatabaseDeleted();
        
        // Create repositories using the test database connection
        _airportRepository = new AirportPostgresqlRepository(_context);
        _repository = new FlightPostgresqlRepository(_context);
        
        // Create default airports for flight tests
        CreateDefaultAirports();
        
        // Track created flights for cleanup
        _createdFlights = new List<FlightDomain>();
    }

    /// <summary>
    /// Creates default airports required for flight tests
    /// </summary>
    private void CreateDefaultAirports()
    {
        var fromAirport = new AirportBuilder().WithId(1).WithName("From Airport").WithCity("From City").WithCountry("From Country").Build();
        var toAirport = new AirportBuilder().WithId(2).WithName("To Airport").WithCity("To City").WithCountry("To Country").Build();
        
        _airportRepository.CreateAsync(fromAirport).Wait();
        _airportRepository.CreateAsync(toAirport).Wait();
    }

    #region IDisposable Implementation

    public void Dispose()
    {
        try
        {
            // Clean up all data
            var flights = _context.Flights.ToList();
            if (flights.Any())
            {
                _context.Flights.RemoveRange(flights);
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
    /// EP1: Valid existing ID - flight exists in database
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_FlightExists_ShouldReturnFlight()
    {
        // Arrange
        var expectedFlight = FlightMother.CreateValidFlight();
        await _repository.CreateAsync(expectedFlight);
        _createdFlights.Add(expectedFlight);

        // Act
        var actualFlight = await _repository.GetByIdAsync(expectedFlight.Id);

        // Assert
        Assert.NotNull(actualFlight);
        Assert.Equal(expectedFlight.Id, actualFlight.Id);
        Assert.Equal(expectedFlight.FlightNumber, actualFlight.FlightNumber);
        Assert.Equal(expectedFlight.DateTime, actualFlight.DateTime);
        Assert.Equal(expectedFlight.FromAirportId, actualFlight.FromAirportId);
        Assert.Equal(expectedFlight.ToAirportId, actualFlight.ToAirportId);
        Assert.Equal(expectedFlight.Price, actualFlight.Price);
    }

    /// <summary>
    /// EP2: Non-existing ID - flight not found
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_FlightNotFound_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var nonExistingId = 999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _repository.GetByIdAsync(nonExistingId));
        
        Assert.Equal(nonExistingId, exception.FlightId);
    }

    /// <summary>
    /// EP3: Flight with zero price - boundary case
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_FlightWithZeroPrice_ShouldReturnFlight()
    {
        // Arrange
        var expectedFlight = FlightMother.CreateFlightWithZeroPrice();
        await _repository.CreateAsync(expectedFlight);
        _createdFlights.Add(expectedFlight);

        // Act
        var actualFlight = await _repository.GetByIdAsync(expectedFlight.Id);

        // Assert
        Assert.NotNull(actualFlight);
        Assert.Equal(0, actualFlight.Price);
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
        var flights = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(flights);
        Assert.Empty(flights);
    }

    /// <summary>
    /// EP2: Single flight - should return list with one flight
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SingleFlight_ShouldReturnListWithOneFlight()
    {
        // Arrange
        var expectedFlight = FlightMother.CreateValidFlight();
        await _repository.CreateAsync(expectedFlight);
        _createdFlights.Add(expectedFlight);

        // Act
        var flights = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(flights);
        Assert.Single(flights);
        Assert.Equal(expectedFlight.Id, flights[0].Id);
    }

    /// <summary>
    /// EP3: Multiple flights - should return all flights
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultipleFlights_ShouldReturnAllFlights()
    {
        // Arrange
        var expectedFlights = FlightMother.CreateFlightList(5);
        foreach (var flight in expectedFlights)
        {
            await _repository.CreateAsync(flight);
            _createdFlights.Add(flight);
        }

        // Act
        var flights = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(flights);
        Assert.Equal(5, flights.Count);
    }

    /// <summary>
    /// EP4: Filter by flight number - should return matching flights
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFlightNumberFilter_ShouldReturnMatchingFlights()
    {
        // Arrange
        var flightSU1234 = new FlightBuilder().WithId(1).WithFlightNumber("SU1234").Build();
        var flightSU5678 = new FlightBuilder().WithId(2).WithFlightNumber("SU5678").Build();
        var flightAA1234 = new FlightBuilder().WithId(3).WithFlightNumber("AA1234").Build();
        
        await _repository.CreateAsync(flightSU1234);
        await _repository.CreateAsync(flightSU5678);
        await _repository.CreateAsync(flightAA1234);
        _createdFlights.AddRange(new[] { flightSU1234, flightSU5678, flightAA1234 });

        // Act
        var flights = await _repository.GetAllAsync(new FlightFilter { FlightNumber = "SU" });

        // Assert
        Assert.NotNull(flights);
        Assert.Equal(2, flights.Count);
        Assert.All(flights, f => Assert.StartsWith("SU", f.FlightNumber));
    }

    /// <summary>
    /// EP5: Filter by date range - should return flights in range
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithDateRangeFilter_ShouldReturnFlightsInRange()
    {
        // Arrange
        var pastFlight = FlightMother.CreatePastFlight();
        var futureFlight1 = new FlightBuilder().WithId(2).WithDateTime(DateTime.UtcNow.AddDays(10)).Build();
        var futureFlight2 = new FlightBuilder().WithId(3).WithDateTime(DateTime.UtcNow.AddDays(20)).Build();
        
        await _repository.CreateAsync(pastFlight);
        await _repository.CreateAsync(futureFlight1);
        await _repository.CreateAsync(futureFlight2);
        _createdFlights.AddRange(new[] { pastFlight, futureFlight1, futureFlight2 });

        var startDate = DateTime.UtcNow.AddDays(5);
        var endDate = DateTime.UtcNow.AddDays(15);

        // Act
        var flights = await _repository.GetAllAsync(new FlightFilter 
        { 
            MinDateTime = startDate, 
            MaxDateTime = endDate 
        });

        // Assert
        Assert.NotNull(flights);
        Assert.Single(flights);
        Assert.All(flights, f => Assert.InRange(f.DateTime, startDate, endDate));
    }

    /// <summary>
    /// EP6: Filter with no matches - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilterNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        await _repository.CreateAsync(flight);
        _createdFlights.Add(flight);

        // Act
        var flights = await _repository.GetAllAsync(new FlightFilter { FlightNumber = "NONEXISTENT" });

        // Assert
        Assert.NotNull(flights);
        Assert.Empty(flights);
    }

    /// <summary>
    /// EP7: Null filter - should return all flights
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithNullFilter_ShouldReturnAllFlights()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(3);
        foreach (var flight in flights)
        {
            await _repository.CreateAsync(flight);
            _createdFlights.Add(flight);
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
    /// EP1: Valid flight with all fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidFlight_ShouldCreateFlight()
    {
        // Arrange
        var flightToCreate = FlightMother.CreateValidFlight();

        // Act
        var createdFlight = await _repository.CreateAsync(flightToCreate);
        _createdFlights.Add(createdFlight);

        // Assert
        Assert.NotNull(createdFlight);
        Assert.NotEqual(0, createdFlight.Id);
        Assert.Equal(flightToCreate.FlightNumber, createdFlight.FlightNumber);
        Assert.Equal(flightToCreate.DateTime, createdFlight.DateTime);
        Assert.Equal(flightToCreate.FromAirportId, createdFlight.FromAirportId);
        Assert.Equal(flightToCreate.ToAirportId, createdFlight.ToAirportId);
        Assert.Equal(flightToCreate.Price, createdFlight.Price);
    }

    /// <summary>
    /// EP2: Valid flight with zero price - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_FlightWithZeroPrice_ShouldCreateFlight()
    {
        // Arrange
        var flightToCreate = FlightMother.CreateFlightWithZeroPrice();

        // Act
        var createdFlight = await _repository.CreateAsync(flightToCreate);
        _createdFlights.Add(createdFlight);

        // Assert
        Assert.NotNull(createdFlight);
        Assert.Equal(0, createdFlight.Price);
    }

    /// <summary>
    /// EP4: Duplicate ID - should throw FlightAlreadyExistsException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_DuplicateId_ShouldThrowFlightAlreadyExistsException()
    {
        // Arrange
        var flight1 = new FlightBuilder().WithId(1).WithFlightNumber("FL1").Build();
        var flight2 = new FlightBuilder().WithId(1).WithFlightNumber("FL2").Build();
        
        await _repository.CreateAsync(flight1);
        _createdFlights.Add(flight1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightAlreadyExistsException>(
            () => _repository.CreateAsync(flight2));
        
        Assert.Equal(1, exception.FlightId);
    }

    /// <summary>
    /// EP5: Flight with max price value - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_FlightWithMaxPrice_ShouldCreateFlight()
    {
        // Arrange
        var flightToCreate = FlightMother.CreateFlightWithMaxPrice();

        // Act
        var createdFlight = await _repository.CreateAsync(flightToCreate);
        _createdFlights.Add(createdFlight);

        // Assert
        Assert.NotNull(createdFlight);
        Assert.Equal(int.MaxValue, createdFlight.Price);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid update with all fields - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_ValidUpdate_ShouldUpdateFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        await _repository.CreateAsync(flight);
        _createdFlights.Add(flight);

        flight.FlightNumber = "UPDATED123";
        flight.DateTime = DateTime.UtcNow.AddDays(30);
        flight.Price = 50000;

        // Act
        var updatedFlight = await _repository.UpdateAsync(flight);

        // Assert
        Assert.NotNull(updatedFlight);
        Assert.Equal("UPDATED123", updatedFlight.FlightNumber);
        Assert.Equal(50000, updatedFlight.Price);
    }

    /// <summary>
    /// EP2: Update price to zero - boundary case
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_UpdatePriceToZero_ShouldUpdateFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        await _repository.CreateAsync(flight);
        _createdFlights.Add(flight);

        flight.Price = 0;

        // Act
        var updatedFlight = await _repository.UpdateAsync(flight);

        // Assert
        Assert.NotNull(updatedFlight);
        Assert.Equal(0, updatedFlight.Price);
    }

    /// <summary>
    /// EP3: Non-existing ID - should throw FlightNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_FlightNotFound_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var flight = new FlightBuilder().WithId(999).WithFlightNumber("NonExistent").Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _repository.UpdateAsync(flight));
        
        Assert.Equal(999, exception.FlightId);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return true and delete
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_FlightExists_ShouldReturnTrueAndDelete()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        await _repository.CreateAsync(flight);
        _createdFlights.Add(flight);

        // Act
        var result = await _repository.DeleteAsync(flight.Id);

        // Assert
        Assert.True(result);
        
        // Verify flight is deleted
        var exists = await _repository.ExistsAsync(flight.Id);
        Assert.False(exists);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_FlightNotFound_ShouldReturnFalse()
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
        var flight = FlightMother.CreateValidFlight();
        await _repository.CreateAsync(flight);
        _createdFlights.Add(flight);

        // Act
        await _repository.DeleteAsync(flight.Id);

        // Assert
        await Assert.ThrowsAsync<FlightNotFoundException>(
            () => _repository.GetByIdAsync(flight.Id));
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Existing ID - should return true
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_FlightExists_ShouldReturnTrue()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        await _repository.CreateAsync(flight);
        _createdFlights.Add(flight);

        // Act
        var exists = await _repository.ExistsAsync(flight.Id);

        // Assert
        Assert.True(exists);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_FlightNotFound_ShouldReturnFalse()
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
        var flight = FlightMother.CreateValidFlight();
        await _repository.CreateAsync(flight);
        _createdFlights.Add(flight);
        
        await _repository.DeleteAsync(flight.Id);

        // Act
        var exists = await _repository.ExistsAsync(flight.Id);

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
    /// EP2: Single flight - should return 1
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_SingleFlight_ShouldReturnOne()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        await _repository.CreateAsync(flight);
        _createdFlights.Add(flight);

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(1, count);
    }

    /// <summary>
    /// EP3: Multiple flights - should return correct count
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_MultipleFlights_ShouldReturnCorrectCount()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(7);
        foreach (var flight in flights)
        {
            await _repository.CreateAsync(flight);
            _createdFlights.Add(flight);
        }

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(7, count);
    }

    #endregion
}
