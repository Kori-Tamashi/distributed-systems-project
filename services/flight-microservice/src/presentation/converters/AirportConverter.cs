using presentation.dto.http.Airport;

namespace presentation.converters;

/// <summary>
/// Converter for Airport between domain and DTO representations
/// </summary>
public static class AirportConverter
{
    /// <summary>
    /// Converts domain Airport to AirportDTO
    /// </summary>
    public static AirportDTO ToDTO(core.domain.Airport airport)
    {
        return new AirportDTO
        {
            Id = airport.Id,
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country
        };
    }

    /// <summary>
    /// Converts AirportDTO to domain Airport
    /// </summary>
    public static core.domain.Airport ToDomain(AirportDTO dto)
    {
        return new core.domain.Airport
        {
            Id = dto.Id,
            Name = dto.Name,
            City = dto.City,
            Country = dto.Country
        };
    }

    /// <summary>
    /// Converts list of domain Airports to list of AirportDTOs
    /// </summary>
    public static List<AirportDTO> ToDTO(List<core.domain.Airport> airports)
    {
        return airports.Select(ToDTO).ToList();
    }

    /// <summary>
    /// Converts list of AirportDTOs to list of domain Airports
    /// </summary>
    public static List<core.domain.Airport> ToDomain(List<AirportDTO> dtos)
    {
        return dtos.Select(ToDomain).ToList();
    }

    /// <summary>
    /// Converts domain Airport to CreateAirportDTO
    /// </summary>
    public static CreateAirportDTO ToCreateDTO(core.domain.Airport airport)
    {
        return new CreateAirportDTO
        {
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country
        };
    }

    /// <summary>
    /// Converts CreateAirportDTO to domain Airport
    /// </summary>
    public static core.domain.Airport ToCreateDomain(CreateAirportDTO dto)
    {
        return new core.domain.Airport
        {
            Id = 0,
            Name = dto.Name,
            City = dto.City,
            Country = dto.Country
        };
    }

    /// <summary>
    /// Converts domain Airport to UpdateAirportDTO
    /// </summary>
    public static UpdateAirportDTO ToUpdateDTO(core.domain.Airport airport)
    {
        return new UpdateAirportDTO(airport.Id)
        {
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country
        };
    }

    /// <summary>
    /// Converts UpdateAirportDTO to domain Airport
    /// Only updates non-null properties
    /// </summary>
    public static core.domain.Airport ToUpdateDomain(UpdateAirportDTO dto, core.domain.Airport existingAirport)
    {
        existingAirport.Name = dto.Name ?? existingAirport.Name;
        existingAirport.City = dto.City ?? existingAirport.City;
        existingAirport.Country = dto.Country ?? existingAirport.Country;

        return existingAirport;
    }

    /// <summary>
    /// Converts domain Airport to UpdateAirportDTO (without existing airport)
    /// </summary>
    public static UpdateAirportDTO ToUpdateDTOWithoutMerge(core.domain.Airport airport)
    {
        return new UpdateAirportDTO(airport.Id)
        {
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country
        };
    }

    /// <summary>
    /// Converts domain Airport to UpdateAirportDTO with only changed properties
    /// </summary>
    public static UpdateAirportDTO ToUpdateDTOPartial(core.domain.Airport airport, params string[] changedProperties)
    {
        var dto = new UpdateAirportDTO(airport.Id);

        if (Array.Exists(changedProperties, p => p.Equals("Name", StringComparison.OrdinalIgnoreCase)))
            dto.Name = airport.Name;

        if (Array.Exists(changedProperties, p => p.Equals("City", StringComparison.OrdinalIgnoreCase)))
            dto.City = airport.City;

        if (Array.Exists(changedProperties, p => p.Equals("Country", StringComparison.OrdinalIgnoreCase)))
            dto.Country = airport.Country;

        return dto;
    }

    /// <summary>
    /// Converts domain Airport to UpdateAirportDTO with only changed properties (nullable check)
    /// </summary>
    public static UpdateAirportDTO ToUpdateDTOPartialNullable(core.domain.Airport airport, params bool[] propertyChanged)
    {
        var dto = new UpdateAirportDTO(airport.Id);

        if (propertyChanged.Length >= 1 && propertyChanged[0])
            dto.Name = airport.Name;

        if (propertyChanged.Length >= 2 && propertyChanged[1])
            dto.City = airport.City;

        if (propertyChanged.Length >= 3 && propertyChanged[2])
            dto.Country = airport.Country;

        return dto;
    }
}
