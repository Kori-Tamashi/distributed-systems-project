using core.domain;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Flight;
using tests.config.attributes;
using tests.fixtures.mothers;

namespace tests.presentation.converters.unit;

/// <summary>
/// Unit tests for FlightHttpConverter
/// Using London-style testing (pure unit tests, no dependencies to mock)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid flight with all fields (normal case)
/// - EP2: Flight with minimal data
/// - EP3: Flight with maximum values
/// - EP4: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid flight to create DTO
/// - EP2: Create DTO should have Id = 0
/// - EP3: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid flight to update DTO
/// - EP2: Update DTO preserves ID
/// - EP3: UpdateDomain merges nullable properties correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of flights
/// - EP2: Empty list handling
/// 
/// Total: 12 unit tests (all should pass)
/// </summary>
public class FlightHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid flight with all fields - should convert correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ValidFlightWithAllFields_ShouldConvertCorrectly()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();

        // Act
        var dto = FlightHttpConverter.ToDTO(flight);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(flight.Id, dto.Id);
        Assert.Equal(flight.FlightNumber, dto.FlightNumber);
        Assert.Equal(flight.FlightUid, dto.FlightUid);
        Assert.Equal(flight.DateTime, dto.DateTime);
        Assert.Equal(flight.FromAirportId, dto.FromAirportId);
        Assert.Equal(flight.ToAirportId, dto.ToAirportId);
        Assert.Equal(flight.Price, dto.Price);
    }

    /// <summary>
    /// EP2: Flight with minimal data - should handle correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_MinimalFlight_ShouldConvertCorrectly()
    {
        // Arrange
        var flight = FlightMother.CreateMinimalFlight();

        // Act
        var dto = FlightHttpConverter.ToDTO(flight);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(flight.Id, dto.Id);
        Assert.Equal(flight.FlightNumber, dto.FlightNumber);
        Assert.Equal(flight.Price, dto.Price); // Price = 0
    }

    /// <summary>
    /// EP3: Flight with maximum values - should handle correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_FlightWithMaxValues_ShouldConvertCorrectly()
    {
        // Arrange
        var flight = FlightMother.CreateFlightWithMaxPrice();

        // Act
        var dto = FlightHttpConverter.ToDTO(flight);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(flight.Id, dto.Id);
        Assert.Equal(int.MaxValue, dto.Price);
    }

    /// <summary>
    /// EP4: ToDomain should convert DTO back to domain entity
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_ValidDto_ShouldConvertToDomainEntity()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        var dto = new FlightDTO
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            FlightUid = flight.FlightUid,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = flight.Price
        };

        // Act
        var result = FlightHttpConverter.ToDomain(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.FlightNumber, result.FlightNumber);
        Assert.Equal(dto.FlightUid, result.FlightUid);
        Assert.Equal(dto.DateTime, result.DateTime);
        Assert.Equal(dto.FromAirportId, result.FromAirportId);
        Assert.Equal(dto.ToAirportId, result.ToAirportId);
        Assert.Equal(dto.Price, result.Price);
    }

    /// <summary>
    /// EP5: Round-trip conversion should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ToDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();

        // Act
        var dto = FlightHttpConverter.ToDTO(flight);
        var result = FlightHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(flight.Id, result.Id);
        Assert.Equal(flight.FlightNumber, result.FlightNumber);
        Assert.Equal(flight.FlightUid, result.FlightUid);
        Assert.Equal(flight.DateTime, result.DateTime);
        Assert.Equal(flight.FromAirportId, result.FromAirportId);
        Assert.Equal(flight.ToAirportId, result.ToAirportId);
        Assert.Equal(flight.Price, result.Price);
    }

    #endregion

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: Valid flight to create DTO should map all fields
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ValidFlight_ShouldMapAllFields()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();

        // Act
        var dto = FlightHttpConverter.ToCreateDTO(flight);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(flight.FlightNumber, dto.FlightNumber);
        Assert.Equal(flight.FlightUid, dto.FlightUid);
        Assert.Equal(flight.DateTime, dto.DateTime);
        Assert.Equal(flight.FromAirportId, dto.FromAirportId);
        Assert.Equal(flight.ToAirportId, dto.ToAirportId);
        Assert.Equal(flight.Price, dto.Price);
    }

    /// <summary>
    /// EP2: ToCreateDomain should set Id to 0
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDomain_CreateDto_ShouldSetIdToZero()
    {
        // Arrange
        var dto = new CreateFlightDTO
        {
            FlightNumber = "SU1234",
            FlightUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow.AddDays(1),
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 15000
        };

        // Act
        var flight = FlightHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(0, flight.Id);
        Assert.Equal(dto.FlightNumber, flight.FlightNumber);
        Assert.Equal(dto.Price, flight.Price);
    }

    /// <summary>
    /// EP3: Create DTO round-trip should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ToCreateDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();

        // Act
        var dto = FlightHttpConverter.ToCreateDTO(flight);
        var result = FlightHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(0, result.Id);
        Assert.Equal(flight.FlightNumber, result.FlightNumber);
        Assert.Equal(flight.FlightUid, result.FlightUid);
        Assert.Equal(flight.DateTime, result.DateTime);
        Assert.Equal(flight.FromAirportId, result.FromAirportId);
        Assert.Equal(flight.ToAirportId, result.ToAirportId);
        Assert.Equal(flight.Price, result.Price);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: Valid flight to update DTO should map all fields including Id
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDTO_ValidFlight_ShouldMapAllFieldsIncludingId()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();

        // Act
        var dto = FlightHttpConverter.ToUpdateDTO(flight);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(flight.Id, dto.Id);
        Assert.Equal(flight.FlightNumber, dto.FlightNumber);
        Assert.Equal(flight.FlightUid, dto.FlightUid);
        Assert.Equal(flight.DateTime, dto.DateTime);
        Assert.Equal(flight.FromAirportId, dto.FromAirportId);
        Assert.Equal(flight.ToAirportId, dto.ToAirportId);
        Assert.Equal(flight.Price, dto.Price);
    }

    /// <summary>
    /// EP2: ToUpdateDomain should preserve the Id and merge nullable properties
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_UpdateDto_ShouldPreserveIdAndMergeProperties()
    {
        // Arrange
        var existingFlight = FlightMother.CreateValidFlight();
        var originalId = existingFlight.Id;
        var originalNumber = existingFlight.FlightNumber;
        
        var dto = new UpdateFlightDTO(existingFlight.Id)
        {
            FlightNumber = "Updated123", // Change only flight number
            Price = 25000                // Change price
        };

        // Act
        var result = FlightHttpConverter.ToUpdateDomain(dto, existingFlight);

        // Assert
        Assert.Equal(originalId, result.Id); // ID preserved
        Assert.Equal("Updated123", result.FlightNumber); // Updated
        Assert.Equal(originalNumber, originalNumber); // Original saved
        Assert.Equal(25000, result.Price); // Updated
    }

    /// <summary>
    /// EP3: ToUpdateDomain with null properties should keep original values
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_WithNullProperties_ShouldKeepOriginalValues()
    {
        // Arrange
        var existingFlight = FlightMother.CreateValidFlight();
        var originalNumber = existingFlight.FlightNumber;
        var originalPrice = existingFlight.Price;
        
        var dto = new UpdateFlightDTO(existingFlight.Id);
        // All properties are null

        // Act
        var result = FlightHttpConverter.ToUpdateDomain(dto, existingFlight);

        // Assert
        Assert.Equal(originalNumber, result.FlightNumber); // Unchanged
        Assert.Equal(originalPrice, result.Price); // Unchanged
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: ToListDTO should convert flights correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ListOfFlights_ShouldConvertAll()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(5);

        // Act
        var dtos = FlightHttpConverter.ToDTO(flights);

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(flights.Count, dtos.Count);
        for (int i = 0; i < flights.Count; i++)
        {
            Assert.Equal(flights[i].Id, dtos[i].Id);
            Assert.Equal(flights[i].FlightNumber, dtos[i].FlightNumber);
        }
    }

    /// <summary>
    /// EP2: ToListDTO with empty list should return empty list
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_EmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var flights = new List<Flight>();

        // Act
        var dtos = FlightHttpConverter.ToDTO(flights);

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    /// <summary>
    /// EP3: ToDomain list should convert all DTOs
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_ListOfDtos_ShouldConvertAll()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(5);
        var dtos = FlightHttpConverter.ToDTO(flights);

        // Act
        var result = FlightHttpConverter.ToDomain(dtos);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(flights.Count, result.Count);
        for (int i = 0; i < flights.Count; i++)
        {
            Assert.Equal(flights[i].Id, result[i].Id);
            Assert.Equal(flights[i].FlightNumber, result[i].FlightNumber);
        }
    }

    #endregion
}
