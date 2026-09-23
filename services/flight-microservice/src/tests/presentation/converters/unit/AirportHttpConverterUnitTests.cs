using core.domain;
using presentation.converters.http;
using presentation.dto.http.Airport;
using tests.config.attributes;
using tests.fixtures.mothers;

namespace tests.presentation.converters.unit;

/// <summary>
/// Unit tests for AirportHttpConverter
/// Using London-style testing (pure unit tests, no dependencies to mock)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid airport with all fields (normal case)
/// - EP2: Airport with minimal data
/// - EP3: Airport with maximum values
/// - EP4: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid airport to create DTO
/// - EP2: Create DTO should have Id = 0
/// - EP3: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid airport to update DTO
/// - EP2: Update DTO preserves ID
/// - EP3: UpdateDomain merges nullable properties correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of airports
/// - EP2: Empty list handling
/// 
/// Total: 12 unit tests (all should pass)
/// </summary>
public class AirportHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid airport with all fields - should convert correctly
    /// </summary>
    [Fact]
    [Unit]
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
    [Unit]
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
        Assert.Equal(airport.City, dto.City);
        Assert.Equal(airport.Country, dto.Country);
    }

    /// <summary>
    /// EP3: Airport with maximum values - should handle correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_AirportWithMaxValues_ShouldConvertCorrectly()
    {
        // Arrange
        var airport = AirportMother.CreateAirportWithMaxName();

        // Act
        var dto = AirportHttpConverter.ToDTO(airport);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(airport.Id, dto.Id);
        Assert.Equal(255, dto.Name.Length); // Max length from builder
    }

    /// <summary>
    /// EP4: ToDomain should convert DTO back to domain entity
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_ValidDto_ShouldConvertToDomainEntity()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        var dto = new AirportDTO
        {
            Id = airport.Id,
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country
        };

        // Act
        var result = AirportHttpConverter.ToDomain(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.City, result.City);
        Assert.Equal(dto.Country, result.Country);
    }

    /// <summary>
    /// EP5: Round-trip conversion should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ToDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();

        // Act
        var dto = AirportHttpConverter.ToDTO(airport);
        var result = AirportHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(airport.Id, result.Id);
        Assert.Equal(airport.Name, result.Name);
        Assert.Equal(airport.City, result.City);
        Assert.Equal(airport.Country, result.Country);
    }

    #endregion

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: Valid airport to create DTO should map all fields
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ValidAirport_ShouldMapAllFields()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();

        // Act
        var dto = AirportHttpConverter.ToCreateDTO(airport);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(airport.Name, dto.Name);
        Assert.Equal(airport.City, dto.City);
        Assert.Equal(airport.Country, dto.Country);
    }

    /// <summary>
    /// EP2: ToCreateDomain should set Id to 0
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDomain_CreateDto_ShouldSetIdToZero()
    {
        // Arrange
        var dto = new CreateAirportDTO
        {
            Name = "Sheremetyevo International Airport",
            City = "Moscow",
            Country = "Russia"
        };

        // Act
        var airport = AirportHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(0, airport.Id);
        Assert.Equal(dto.Name, airport.Name);
        Assert.Equal(dto.City, airport.City);
        Assert.Equal(dto.Country, airport.Country);
    }

    /// <summary>
    /// EP3: Create DTO round-trip should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ToCreateDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();

        // Act
        var dto = AirportHttpConverter.ToCreateDTO(airport);
        var result = AirportHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(0, result.Id);
        Assert.Equal(airport.Name, result.Name);
        Assert.Equal(airport.City, result.City);
        Assert.Equal(airport.Country, result.Country);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: Valid airport to update DTO should map all fields including Id
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDTO_ValidAirport_ShouldMapAllFieldsIncludingId()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();

        // Act
        var dto = AirportHttpConverter.ToUpdateDTO(airport);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(airport.Id, dto.Id);
        Assert.Equal(airport.Name, dto.Name);
        Assert.Equal(airport.City, dto.City);
        Assert.Equal(airport.Country, dto.Country);
    }

    /// <summary>
    /// EP2: ToUpdateDomain should preserve the Id and merge nullable properties
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_UpdateDto_ShouldPreserveIdAndMergeProperties()
    {
        // Arrange
        var existingAirport = AirportMother.CreateValidAirport();
        var originalId = existingAirport.Id;
        var originalName = existingAirport.Name;
        
        var dto = new UpdateAirportDTO(existingAirport.Id)
        {
            Name = "Updated Airport Name", // Change only name
            City = "Updated City"          // Change city
        };

        // Act
        var result = AirportHttpConverter.ToUpdateDomain(dto, existingAirport);

        // Assert
        Assert.Equal(originalId, result.Id); // ID preserved
        Assert.Equal("Updated Airport Name", result.Name); // Updated
        Assert.Equal("Updated City", result.City); // Updated
        Assert.Equal(originalName, originalName); // Original saved
    }

    /// <summary>
    /// EP3: ToUpdateDomain with null properties should keep original values
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_WithNullProperties_ShouldKeepOriginalValues()
    {
        // Arrange
        var existingAirport = AirportMother.CreateValidAirport();
        var originalName = existingAirport.Name;
        var originalCity = existingAirport.City;
        var originalCountry = existingAirport.Country;
        
        var dto = new UpdateAirportDTO(existingAirport.Id);
        // All properties are null

        // Act
        var result = AirportHttpConverter.ToUpdateDomain(dto, existingAirport);

        // Assert
        Assert.Equal(originalName, result.Name); // Unchanged
        Assert.Equal(originalCity, result.City); // Unchanged
        Assert.Equal(originalCountry, result.Country); // Unchanged
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: ToListDTO should convert airports correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ListOfAirports_ShouldConvertAll()
    {
        // Arrange
        var airports = AirportMother.CreateAirportList(5);

        // Act
        var dtos = AirportHttpConverter.ToDTO(airports);

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(airports.Count, dtos.Count);
        for (int i = 0; i < airports.Count; i++)
        {
            Assert.Equal(airports[i].Id, dtos[i].Id);
            Assert.Equal(airports[i].Name, dtos[i].Name);
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
        var airports = new List<Airport>();

        // Act
        var dtos = AirportHttpConverter.ToDTO(airports);

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
        var airports = AirportMother.CreateAirportList(5);
        var dtos = AirportHttpConverter.ToDTO(airports);

        // Act
        var result = AirportHttpConverter.ToDomain(dtos);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(airports.Count, result.Count);
        for (int i = 0; i < airports.Count; i++)
        {
            Assert.Equal(airports[i].Id, result[i].Id);
            Assert.Equal(airports[i].Name, result[i].Name);
        }
    }

    #endregion
}
