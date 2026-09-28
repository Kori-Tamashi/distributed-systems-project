using dataaccess.dto.http;
using dataaccess.dto.http.Airport;

namespace dataaccess.converters.http;

/// <summary>
/// Converter for Airport between domain and HTTP DTO representations
/// </summary>
public static class AirportHttpConverter
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
    /// Converts CreateAirportDTO to domain Airport
    /// </summary>
    public static core.domain.Airport ToDomain(CreateAirportDTO dto)
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
    /// Converts UpdateAirportDTO to domain Airport
    /// </summary>
    public static core.domain.Airport ToDomain(UpdateAirportDTO dto)
    {
        return new core.domain.Airport
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            City = dto.City ?? string.Empty,
            Country = dto.Country ?? string.Empty
        };
    }

    /// <summary>
    /// Converts domain Airport to UpdateAirportDTO
    /// </summary>
    public static UpdateAirportDTO ToUpdateDTO(core.domain.Airport airport)
    {
        return new UpdateAirportDTO
        {
            Id = airport.Id,
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country
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
    /// Converts IEnumerable of domain Airports to IEnumerable of AirportDTOs
    /// </summary>
    public static IEnumerable<AirportDTO> ToDTO(IEnumerable<core.domain.Airport> airports)
    {
        return airports.Select(ToDTO);
    }
}
