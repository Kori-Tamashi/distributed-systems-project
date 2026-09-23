using presentation.converters.http;
using presentation.dto.http.Flight;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit.http;

/// <summary>
/// Unit tests for presentation FlightHttpConverter
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid flight with all fields (normal case)
/// - EP2: Flight with minimal data
/// - EP3: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid flight to create DTO
/// - EP2: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid flight to update DTO
/// - EP2: UpdateDomain handles nullable fields correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of flights
/// - EP2: Empty list handling
/// 
/// Total: 10 unit tests (all should pass)
/// </summary>
public class FlightHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid flight with all fields - should convert correctly
    /// </summary>
    [Fact]
    public void ToDTO_ValidFlightWithAllFields_ShouldConvertCorrectly()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();

        // Act
        var dto = FlightHttpConverter.ToDTO(flight);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(flight.Id, dto.Id);
        Assert.Equal(flight.FlightUid, dto.FlightUid);
        Assert.Equal(flight.FlightNumber, dto.FlightNumber);
        Assert.Equal(flight.DateTime, dto.DateTime);
        Assert.Equal(flight.FromAirportId, dto.FromAirportId);
        Assert.Equal(flight.ToAirportId, dto.ToAirportId);
        Assert.Equal(flight.Price, dto.Price);
    }

    /// <summary>
    /// EP2: Flight with minimal data - should handle correctly
    /// </summary>
    [Fact]
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
    }

    /// <summary>
    /// EP3: Round-trip conversion preserves all data
    /// </summary>
    [Fact]
    public void ToDTO_ToDomain_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalFlight = FlightMother.CreateValidFlight();

        // Act
        var dto = FlightHttpConverter.ToDTO(originalFlight);
        var convertedFlight = FlightHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(originalFlight.Id, convertedFlight.Id);
        Assert.Equal(originalFlight.FlightUid, convertedFlight.FlightUid);
        Assert.Equal(originalFlight.FlightNumber, convertedFlight.FlightNumber);
        Assert.Equal(originalFlight.DateTime, convertedFlight.DateTime);
        Assert.Equal(originalFlight.FromAirportId, convertedFlight.FromAirportId);
        Assert.Equal(originalFlight.ToAirportId, convertedFlight.ToAirportId);
        Assert.Equal(originalFlight.Price, convertedFlight.Price);
    }

    #endregion

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToCreateDTO_ValidFlight_ShouldConvertCorrectly()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();

        // Act
        var createDto = FlightHttpConverter.ToCreateDTO(flight);

        // Assert
        Assert.NotNull(createDto);
        Assert.Equal(flight.FlightNumber, createDto.FlightNumber);
        Assert.Equal(flight.DateTime, createDto.DateTime);
        Assert.Equal(flight.FromAirportId, createDto.FromAirportId);
        Assert.Equal(flight.ToAirportId, createDto.ToAirportId);
        Assert.Equal(flight.Price, createDto.Price);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    [Fact]
    public void ToCreateDomain_CreateFlightDTO_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalFlight = FlightMother.CreateValidFlight();
        var createDto = new CreateFlightDTO
        {
            FlightNumber = originalFlight.FlightNumber,
            DateTime = originalFlight.DateTime,
            FromAirportId = originalFlight.FromAirportId,
            ToAirportId = originalFlight.ToAirportId,
            Price = originalFlight.Price
        };

        // Act
        var flight = FlightHttpConverter.ToCreateDomain(createDto);

        // Assert
        Assert.Equal(originalFlight.FlightNumber, flight.FlightNumber);
        Assert.Equal(originalFlight.DateTime, flight.DateTime);
        Assert.Equal(originalFlight.Price, flight.Price);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToUpdateDTO_ValidFlight_ShouldConvertCorrectly()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();

        // Act
        var updateDto = FlightHttpConverter.ToUpdateDTO(flight);

        // Assert
        Assert.NotNull(updateDto);
        Assert.Equal(flight.FlightNumber, updateDto.FlightNumber);
        Assert.Equal(flight.DateTime, updateDto.DateTime);
        Assert.Equal(flight.Price, updateDto.Price);
    }

    /// <summary>
    /// EP2: UpdateDomain handles nullable fields correctly
    /// </summary>
    [Fact]
    public void ToUpdateDomain_UpdateFlightDTO_NullFields_ShouldKeepExistingValues()
    {
        // Arrange
        var existingFlight = FlightMother.CreateValidFlight();
        var updateDto = new UpdateFlightDTO
        {
            FlightNumber = "Updated Flight Number"
            // Other fields are null
        };

        // Act
        var updatedFlight = FlightHttpConverter.ToUpdateDomain(updateDto, existingFlight);

        // Assert
        Assert.Equal("Updated Flight Number", updatedFlight.FlightNumber);
        Assert.Equal(existingFlight.DateTime, updatedFlight.DateTime);
        Assert.Equal(existingFlight.Price, updatedFlight.Price);
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: Convert list of flights
    /// </summary>
    [Fact]
    public void ToDTO_ListOfFlights_ShouldConvertAll()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(3);

        // Act
        var dtos = FlightHttpConverter.ToDTO(flights).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(3, dtos.Count);
        for (int i = 0; i < flights.Count; i++)
        {
            Assert.Equal(flights[i].Id, dtos[i].Id);
            Assert.Equal(flights[i].FlightNumber, dtos[i].FlightNumber);
        }
    }

    /// <summary>
    /// EP2: Empty list handling
    /// </summary>
    [Fact]
    public void ToDTO_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var flights = new List<core.domain.Flight>();

        // Act
        var dtos = FlightHttpConverter.ToDTO(flights).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
