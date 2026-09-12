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

using ServiceFlightNotFoundException = core.exceptions.businesslogic.services.FlightNotFoundException;
using ServiceFlightValidationException = core.exceptions.businesslogic.services.FlightValidationException;
using ServiceAirportNotFoundException = core.exceptions.businesslogic.services.AirportNotFoundException;
using FlightDomain = core.domain.Flight;

namespace tests.businesslogic.services.integration.postgres;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for FlightService
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresqlDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_flights).
/// FlightService is tested with real FlightPostgresqlRepository and AirportPostgresqlRepository.
/// 
/// TEST CONTEXT:
/// - TestPostgresqlDatabaseContext is used for database operations
/// - Database is created once in constructor and cleaned after each test
/// - Each test method cleans up its own data in Dispose
/// - Tests share the same database but are isolated by cleanup
/// - Default airports are created in constructor for Flight foreign key requirements
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetByIdAsync Tests:
///    EP1: Valid existing ID (normal case) - flight exists in database
///    EP2: Non-existing ID (error case) - FlightNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - FlightValidationException thrown
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single flight (normal case) - should return list with one flight
///    EP3: Multiple flights (normal case) - should return all flights
///    EP4: Filter by flight number (normal case) - should return matching flights
///    EP5: Filter by airport (normal case) - should return flights from/to airport
///    EP6: Filter with no matches (edge case) - should return empty list
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid flight with all fields (normal case) - should create successfully
///    EP2: Valid flight with minimal data (edge case) - should create successfully
///    EP3: Flight with empty flight number (validation case) - FlightValidationException thrown
///    EP4: Flight with past date (validation case) - FlightValidationException thrown
///    EP5: Flight with same airports (validation case) - FlightValidationException thrown
///    EP6: Flight with negative price (validation case) - FlightValidationException thrown
///    EP7: Airport not found (error case) - AirportNotFoundException thrown
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Non-existing ID (error case) - FlightNotFoundException thrown
///    EP3: Invalid flight number (validation case) - FlightValidationException thrown
///    EP4: Invalid ID <= 0 (validation case) - FlightValidationException thrown
///    EP5: Airport not found (error case) - AirportNotFoundException thrown
/// 
/// 5. DeleteAsync Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (error case) - FlightNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - FlightValidationException thrown
///    EP4: Delete and verify data removed (verification case) - GetById throws
/// 
/// 6. ExistsAsync Tests:
///    EP1: Existing ID (normal case) - should return true
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: ID after deletion (verification case) - should return false
///    EP4: Invalid ID <= 0 (validation case) - FlightValidationException thrown
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
/// 
/// Total: 34 integration tests
/// </summary>
public class FlightServiceIntegrationTests : IDisposable
{
    private readonly TestPostgresqlDatabaseContext _testContext;
    private readonly IAirportRepository _airportRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IFlightService _service;

    public FlightServiceIntegrationTests()
    {
        // Create test database context - this uses TEST_* environment variables
        _testContext = new TestPostgresqlDatabaseContext();
        _testContext.EnsureDatabaseDeleted(); // Create tables automatically
        
        // Create repositories and service using the test database context
        _airportRepository = new AirportPostgresqlRepository(_testContext);
        _flightRepository = new FlightPostgresqlRepository(_testContext);
        _service = new FlightService(_flightRepository, _airportRepository);
        
        // Create default airports for Flight foreign key requirements
        CreateDefaultAirports();
    }

    public void Dispose()
    {
        try
        {
            // Clean up all flights first (due to foreign key constraints)
            var flights = _testContext.Flights.ToList();
            if (flights.Any())
            {
                _testContext.Flights.RemoveRange(flights);
                _testContext.SaveChanges();
            }
            
            // Then clean up airports
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

    /// <summary>
    /// Create default airports required for Flight foreign keys
    /// </summary>
    private void CreateDefaultAirports()
    {
        var fromAirport = new AirportBuilder()
            .WithId(1)
            .WithName("Sheremetyevo International Airport")
            .WithCity("Moscow")
            .WithCountry("Russia")
            .Build();
            
        var toAirport = new AirportBuilder()
            .WithId(2)
            .WithName("Pulkovo Airport")
            .WithCity("Saint Petersburg")
            .WithCountry("Russia")
            .Build();
        
        _airportRepository.CreateAsync(fromAirport).Wait();
        _airportRepository.CreateAsync(toAirport).Wait();
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return flight
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_FlightExists_ShouldReturnFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = 0;
        var created = await _service.CreateAsync(flight);

        // Act
        var result = await _service.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.FlightNumber, result.FlightNumber);
        Assert.Equal(created.Price, result.Price);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw FlightNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_NonExistingId_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightNotFoundException>(
            () => _service.GetByIdAsync(nonExistentId)
        );
        Assert.Equal(nonExistentId, exception.FlightId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw FlightValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
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
        // Arrange - Database is empty (only default airports)

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// EP2: Single flight - should return list with one flight
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SingleFlight_ShouldReturnListWithOneFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = 0;
        await _service.CreateAsync(flight);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(flight.FlightNumber, result[0].FlightNumber);
    }

    /// <summary>
    /// EP3: Multiple flights - should return all flights
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultipleFlights_ShouldReturnAllFlights()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(5);
        foreach (var flight in flights)
        {
            flight.Id = 0;
            await _service.CreateAsync(flight);
        }

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
    }

    /// <summary>
    /// EP4: Filter by flight number - should return matching flights
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFlightNumberFilter_ShouldReturnMatchingFlights()
    {
        // Arrange
        var flight1 = new FlightBuilder().WithId(0).WithFlightNumber("SU1001").WithFutureDateTime().WithFromAirportId(1).WithToAirportId(2).WithPrice(5000).Build();
        var flight2 = new FlightBuilder().WithId(0).WithFlightNumber("SU2002").WithFutureDateTime().WithFromAirportId(1).WithToAirportId(2).WithPrice(6000).Build();
        var flight3 = new FlightBuilder().WithId(0).WithFlightNumber("AF123").WithFutureDateTime().WithFromAirportId(1).WithToAirportId(2).WithPrice(50000).Build();
        
        await _service.CreateAsync(flight1);
        await _service.CreateAsync(flight2);
        await _service.CreateAsync(flight3);

        var filter = new FlightFilter { FlightNumber = "SU" };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.StartsWith("SU", f.FlightNumber));
    }

    /// <summary>
    /// EP5: Filter by airport - should return flights from/to airport
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithAirportFilter_ShouldReturnMatchingFlights()
    {
        // Arrange
        var flight1 = new FlightBuilder().WithId(0).WithFlightNumber("SU1001").WithFutureDateTime().WithFromAirportId(1).WithToAirportId(2).WithPrice(5000).Build();
        var flight2 = new FlightBuilder().WithId(0).WithFlightNumber("SU2002").WithFutureDateTime().WithFromAirportId(1).WithToAirportId(2).WithPrice(6000).Build();
        var flight3 = new FlightBuilder().WithId(0).WithFlightNumber("AF123").WithFutureDateTime().WithFromAirportId(1).WithToAirportId(2).WithPrice(50000).Build();
        
        await _service.CreateAsync(flight1);
        await _service.CreateAsync(flight2);
        await _service.CreateAsync(flight3);

        var filter = new FlightFilter { FromAirportId = 1 };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, f => Assert.Equal(1, f.FromAirportId));
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
        flight.Id = 0;
        await _service.CreateAsync(flight);

        var filter = new FlightFilter { FlightNumber = "NonExistent" };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid flight with all fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidFlightWithAllFields_ShouldCreateFlight()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = 0;

        // Act
        var result = await _service.CreateAsync(flight);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(flight.FlightNumber, result.FlightNumber);
        Assert.Equal(flight.Price, result.Price);
        Assert.Equal(flight.FromAirportId, result.FromAirportId);
        Assert.Equal(flight.ToAirportId, result.ToAirportId);
        
        // Verify in database
        var retrieved = await _service.GetByIdAsync(result.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(result.Id, retrieved.Id);
    }

    /// <summary>
    /// EP2: Valid flight with minimal data - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidFlightWithMinimalData_ShouldCreateFlight()
    {
        // Arrange
        var flight = new FlightBuilder()
            .WithId(0)
            .WithFlightNumber("SU5678")
            .WithFutureDateTime()
            .WithFromAirportId(1)
            .WithToAirportId(2)
            .WithPrice(1000)
            .Build();

        // Act
        var result = await _service.CreateAsync(flight);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(flight.FlightNumber, result.FlightNumber);
    }

    /// <summary>
    /// EP3: Flight with empty flight number - should throw FlightValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_EmptyFlightNumber_ShouldThrowFlightValidationException()
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
        flight.Id = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("Flight validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Flight with past date - should throw FlightValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_PastDate_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = FlightMother.CreatePastFlight();
        flight.Id = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("Flight validation failed", exception.Message);
    }

    /// <summary>
    /// EP5: Flight with same airports - should throw FlightValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_SameAirports_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = FlightMother.CreateFlightWithSameAirports();
        flight.Id = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("Flight validation failed", exception.Message);
    }

    /// <summary>
    /// EP6: Flight with negative price - should throw FlightValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_NegativePrice_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = FlightMother.CreateFlightWithNegativePrice();
        flight.Id = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightValidationException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Contains("Flight validation failed", exception.Message);
    }

    /// <summary>
    /// EP7: Airport not found - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = 0;
        flight.FromAirportId = 9999; // Non-existent airport

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportNotFoundException>(
            () => _service.CreateAsync(flight)
        );
        Assert.Equal(9999, exception.AirportId);
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
        flight.Id = 0;
        var created = await _service.CreateAsync(flight);
        
        var updatedFlight = new Flight
        {
            Id = created.Id,
            FlightNumber = "SU9999",
            DateTime = DateTime.UtcNow.AddDays(10),
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 25000
        };

        // Act
        var result = await _service.UpdateAsync(updatedFlight);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SU9999", result.FlightNumber);
        Assert.Equal(25000, result.Price);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw FlightNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_FlightNotFound_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightNotFoundException>(
            () => _service.UpdateAsync(flight)
        );
        Assert.Equal(flight.Id, exception.FlightId);
    }

    /// <summary>
    /// EP3: Invalid flight number - should throw FlightValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidFlightNumber_ShouldThrowFlightValidationException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = 0;
        var created = await _service.CreateAsync(flight);
        
        var invalidFlight = new Flight
        {
            Id = created.Id,
            FlightNumber = string.Empty, // Invalid
            DateTime = DateTime.UtcNow.AddDays(1),
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 10000
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightValidationException>(
            () => _service.UpdateAsync(invalidFlight)
        );
        Assert.Contains("Flight validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw FlightValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task UpdateAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightValidationException>(
            () => _service.UpdateAsync(flight)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
    }

    /// <summary>
    /// EP5: Airport not found - should throw AirportNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_AirportNotFound_ShouldThrowAirportNotFoundException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = 0;
        var created = await _service.CreateAsync(flight);
        
        var updatedFlight = new Flight
        {
            Id = created.Id,
            FlightNumber = "SU1234",
            DateTime = DateTime.UtcNow.AddDays(1),
            FromAirportId = 9999, // Non-existent airport
            ToAirportId = 2,
            Price = 10000
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceAirportNotFoundException>(
            () => _service.UpdateAsync(updatedFlight)
        );
        Assert.Equal(9999, exception.AirportId);
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
        flight.Id = 0;
        var created = await _service.CreateAsync(flight);

        // Act
        var result = await _service.DeleteAsync(created.Id);

        // Assert
        Assert.True(result);
        
        // Verify flight is deleted
        var exception = await Assert.ThrowsAsync<ServiceFlightNotFoundException>(
            () => _service.GetByIdAsync(created.Id)
        );
        Assert.Equal(created.Id, exception.FlightId);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw FlightNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_FlightNotFound_ShouldThrowFlightNotFoundException()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightNotFoundException>(
            () => _service.DeleteAsync(nonExistentId)
        );
        Assert.Equal(nonExistentId, exception.FlightId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw FlightValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
    }

    /// <summary>
    /// EP4: Delete and verify data removed - GetById should throw
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_VerifyDataRemoved_ShouldThrowNotFoundException()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        flight.Id = 0;
        var created = await _service.CreateAsync(flight);

        // Act
        await _service.DeleteAsync(created.Id);

        // Assert - Verify data is completely removed
        var exception = await Assert.ThrowsAsync<ServiceFlightNotFoundException>(
            () => _service.GetByIdAsync(created.Id)
        );
        Assert.Equal(created.Id, exception.FlightId);
        
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
    public async Task ExistsAsync_FlightExists_ShouldReturnTrue()
    {
        // Arrange
        var flight = new FlightBuilder().WithId(0).WithFlightNumber("SU1234").WithFutureDateTime().WithFromAirportId(1).WithToAirportId(2).WithPrice(10000).Build();
        var created = await _service.CreateAsync(flight);

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
    public async Task ExistsAsync_FlightNotFound_ShouldReturnFalse()
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
        var flight = new FlightBuilder().WithId(0).WithFlightNumber("SU1234").WithFutureDateTime().WithFromAirportId(1).WithToAirportId(2).WithPrice(10000).Build();
        var created = await _service.CreateAsync(flight);
        await _service.DeleteAsync(created.Id);

        // Act
        var result = await _service.ExistsAsync(created.Id);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw FlightValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task ExistsAsync_InvalidId_ShouldThrowFlightValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceFlightValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Flight ID", exception.Message);
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
        // Arrange - Database is empty (only default airports)

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(0, result);
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
        flight.Id = 0;
        await _service.CreateAsync(flight);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(1, result);
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
            flight.Id = 0;
            await _service.CreateAsync(flight);
        }

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(7, result);
    }

    #endregion
}
