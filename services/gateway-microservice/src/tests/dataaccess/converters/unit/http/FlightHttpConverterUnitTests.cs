using core.domain;
using dataaccess.converters.http;
using dataaccess.dto.http;
using dataaccess.dto.http.Flight;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.converters.unit.http;

/// <summary>
/// Unit tests for FlightHttpConverter
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid flight with all fields (normal case)
/// - EP2: Flight with minimal data
/// - EP3: Flight with maximum price
/// - EP4: Round-trip conversion preserves data
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
        Assert.Equal(flight.Price, dto.Price);
    }

    /// <summary>
    /// EP3: Flight with maximum price - should convert correctly
    /// </summary>
    [Fact]
    public void ToDTO_ExpensiveFlight_ShouldConvertCorrectly()
    {
        // Arrange
        var flight = FlightMother.CreateExpensiveFlight();

        // Act
        var dto = FlightHttpConverter.ToDTO(flight);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(flight.Price, dto.Price);
        Assert.Equal(100000, dto.Price);
    }

    /// <summary>
    /// EP4: Round-trip conversion preserves all data
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
        Assert.Equal(originalFlight.FlightNumber, convertedFlight.FlightNumber);
        Assert.Equal(originalFlight.FlightUid, convertedFlight.FlightUid);
        Assert.Equal(originalFlight.DateTime, convertedFlight.DateTime);
        Assert.Equal(originalFlight.FromAirportId, convertedFlight.FromAirportId);
        Assert.Equal(originalFlight.ToAirportId, convertedFlight.ToAirportId);
        Assert.Equal(originalFlight.Price, convertedFlight.Price);
    }

    #endregion

    #region ToDomain(CreateDTO) Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToDomain_CreateFlightDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var createDto = new CreateFlightDTO
        {
            FlightNumber = "SU200",
            FlightUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow.AddDays(1),
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 20000
        };

        // Act
        var flight = FlightHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(createDto.FlightNumber, flight.FlightNumber);
        Assert.Equal(createDto.FlightUid, flight.FlightUid);
        Assert.Equal(createDto.DateTime, flight.DateTime);
        Assert.Equal(createDto.FromAirportId, flight.FromAirportId);
        Assert.Equal(createDto.ToAirportId, flight.ToAirportId);
        Assert.Equal(createDto.Price, flight.Price);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    [Fact]
    public void ToDomain_CreateFlightDTO_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalFlight = FlightMother.CreateValidFlight();
        var createDto = new CreateFlightDTO
        {
            FlightNumber = originalFlight.FlightNumber,
            FlightUid = originalFlight.FlightUid,
            DateTime = originalFlight.DateTime,
            FromAirportId = originalFlight.FromAirportId,
            ToAirportId = originalFlight.ToAirportId,
            Price = originalFlight.Price
        };

        // Act
        var flight = FlightHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(originalFlight.FlightNumber, flight.FlightNumber);
        Assert.Equal(originalFlight.FlightUid, flight.FlightUid);
        Assert.Equal(originalFlight.DateTime, flight.DateTime);
        Assert.Equal(originalFlight.FromAirportId, flight.FromAirportId);
        Assert.Equal(originalFlight.ToAirportId, flight.ToAirportId);
        Assert.Equal(originalFlight.Price, flight.Price);
    }

    #endregion

    #region ToDomain(UpdateDTO) Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToDomain_UpdateFlightDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var updateDto = new UpdateFlightDTO(1)
        {
            FlightNumber = "SU300",
            Price = 25000
        };

        // Act
        var flight = FlightHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, flight.Id);
        Assert.Equal("SU300", flight.FlightNumber);
        Assert.Equal(25000, flight.Price);
    }

    /// <summary>
    /// EP2: UpdateDTO with nullable fields should handle nulls correctly
    /// </summary>
    [Fact]
    public void ToDomain_UpdateFlightDTO_NullFields_ShouldUseDefaults()
    {
        // Arrange
        var updateDto = new UpdateFlightDTO(1);

        // Act
        var flight = FlightHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, flight.Id);
        Assert.Equal(string.Empty, flight.FlightNumber);
        Assert.Equal(Guid.Empty, flight.FlightUid);
        Assert.Equal(0, flight.Price);
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
        var flights = FlightMother.CreateFlightList(5);

        // Act
        var dtos = FlightHttpConverter.ToDTO(flights).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(5, dtos.Count);
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
        var flights = new List<Flight>();

        // Act
        var dtos = FlightHttpConverter.ToDTO(flights).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
