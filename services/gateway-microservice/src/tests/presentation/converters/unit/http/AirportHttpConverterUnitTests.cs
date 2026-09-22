using presentation.converters.http;
using presentation.dto.http.Airport;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit.http;

/// <summary>
/// Unit tests for presentation AirportHttpConverter
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
    [Fact]
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
    [Fact]
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
    [Fact]
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

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToCreateDTO_ValidAirport_ShouldConvertCorrectly()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();

        // Act
        var createDto = AirportHttpConverter.ToCreateDTO(airport);

        // Assert
        Assert.NotNull(createDto);
        Assert.Equal(airport.Name, createDto.Name);
        Assert.Equal(airport.City, createDto.City);
        Assert.Equal(airport.Country, createDto.Country);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    [Fact]
    public void ToCreateDomain_CreateAirportDTO_RoundTripShouldPreserveData()
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
        var airport = AirportHttpConverter.ToCreateDomain(createDto);

        // Assert
        Assert.Equal(originalAirport.Name, airport.Name);
        Assert.Equal(originalAirport.City, airport.City);
        Assert.Equal(originalAirport.Country, airport.Country);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToUpdateDTO_ValidAirport_ShouldConvertCorrectly()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();

        // Act
        var updateDto = AirportHttpConverter.ToUpdateDTO(airport);

        // Assert
        Assert.NotNull(updateDto);
        Assert.Equal(airport.Name, updateDto.Name);
        Assert.Equal(airport.City, updateDto.City);
        Assert.Equal(airport.Country, updateDto.Country);
    }

    /// <summary>
    /// EP2: UpdateDomain handles nullable fields correctly
    /// </summary>
    [Fact]
    public void ToUpdateDomain_UpdateAirportDTO_NullFields_ShouldKeepExistingValues()
    {
        // Arrange
        var existingAirport = AirportMother.CreateValidAirport();
        var updateDto = new UpdateAirportDTO("Updated Name")
        {
            // City and Country are null
        };

        // Act
        var updatedAirport = AirportHttpConverter.ToUpdateDomain(updateDto, existingAirport);

        // Assert
        Assert.Equal("Updated Name", updatedAirport.Name);
        Assert.Equal(existingAirport.City, updatedAirport.City);
        Assert.Equal(existingAirport.Country, updatedAirport.Country);
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: Convert list of airports
    /// </summary>
    [Fact]
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
    [Fact]
    public void ToDTO_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var airports = new List<core.domain.Airport>();

        // Act
        var dtos = AirportHttpConverter.ToDTO(airports).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
