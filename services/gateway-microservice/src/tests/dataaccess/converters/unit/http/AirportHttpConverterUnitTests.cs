using core.domain;
using dataaccess.converters.http;
using dataaccess.dto.http;
using dataaccess.dto.http.Airport;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.converters.unit.http;

/// <summary>
/// Unit tests for AirportHttpConverter
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid airport with all fields (normal case)
/// - EP2: Airport with minimal data
/// - EP3: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid airport to create DTO
/// - EP2: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid airport to update DTO
/// - EP2: UpdateDomain handles nullable fields correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of airports
/// - EP2: Empty list handling
/// 
/// Total: 10 unit tests (all should pass)
/// </summary>
public class AirportHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid airport with all fields - should convert correctly
    /// </summary>
    public void ToDTO_ValidAirportWithAllFields_ShouldConvertCorrectly()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();

        // Act
        var dto = AirportHttpConverter.ToDTO(airport);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(airport.Id, dto.Id);
        Assert.Equal(airport.Name, dto.Name);
        Assert.Equal(airport.City, dto.City);
        Assert.Equal(airport.Country, dto.Country);
    }

    /// <summary>
    /// EP2: Airport with minimal data - should handle correctly
    /// </summary>
    public void ToDTO_MinimalAirport_ShouldConvertCorrectly()
    {
        // Arrange
        var airport = AirportMother.CreateMinimalAirport();

        // Act
        var dto = AirportHttpConverter.ToDTO(airport);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(airport.Id, dto.Id);
        Assert.Equal(airport.Name, dto.Name);
    }

    /// <summary>
    /// EP3: Round-trip conversion preserves all data
    /// </summary>
    public void ToDTO_ToDomain_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalAirport = AirportMother.CreateValidAirport();

        // Act
        var dto = AirportHttpConverter.ToDTO(originalAirport);
        var convertedAirport = AirportHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(originalAirport.Id, convertedAirport.Id);
        Assert.Equal(originalAirport.Name, convertedAirport.Name);
        Assert.Equal(originalAirport.City, convertedAirport.City);
        Assert.Equal(originalAirport.Country, convertedAirport.Country);
    }

    #endregion

    #region ToDomain(CreateDTO) Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    public void ToDomain_CreateAirportDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var createDto = new CreateAirportDTO
        {
            Name = "Test Airport",
            City = "Test City",
            Country = "Test Country"
        };

        // Act
        var airport = AirportHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(createDto.Name, airport.Name);
        Assert.Equal(createDto.City, airport.City);
        Assert.Equal(createDto.Country, airport.Country);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    public void ToDomain_CreateAirportDTO_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalAirport = AirportMother.CreateValidAirport();
        var createDto = new CreateAirportDTO
        {
            Name = originalAirport.Name,
            City = originalAirport.City,
            Country = originalAirport.Country
        };

        // Act
        var airport = AirportHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(originalAirport.Name, airport.Name);
        Assert.Equal(originalAirport.City, airport.City);
        Assert.Equal(originalAirport.Country, airport.Country);
    }

    #endregion

    #region ToDomain(UpdateDTO) Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    public void ToDomain_UpdateAirportDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var updateDto = new UpdateAirportDTO(1)
        {
            Name = "Updated Airport",
            City = "Updated City"
        };

        // Act
        var airport = AirportHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, airport.Id);
        Assert.Equal("Updated Airport", airport.Name);
        Assert.Equal("Updated City", airport.City);
    }

    /// <summary>
    /// EP2: UpdateDTO with nullable fields should handle nulls correctly
    /// </summary>
    public void ToDomain_UpdateAirportDTO_NullFields_ShouldUseDefaults()
    {
        // Arrange
        var updateDto = new UpdateAirportDTO(1);

        // Act
        var airport = AirportHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, airport.Id);
        Assert.Equal(string.Empty, airport.Name);
        Assert.Equal(string.Empty, airport.City);
        Assert.Equal(string.Empty, airport.Country);
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: Convert list of airports
    /// </summary>
    public void ToDTO_ListOfAirports_ShouldConvertAll()
    {
        // Arrange
        var airports = AirportMother.CreateAirportList(3);

        // Act
        var dtos = AirportHttpConverter.ToDTO(airports).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(3, dtos.Count);
        for (int i = 0; i < airports.Count; i++)
        {
            Assert.Equal(airports[i].Id, dtos[i].Id);
            Assert.Equal(airports[i].Name, dtos[i].Name);
        }
    }

    /// <summary>
    /// EP2: Empty list handling
    /// </summary>
    public void ToDTO_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var airports = new List<Airport>();

        // Act
        var dtos = AirportHttpConverter.ToDTO(airports).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
