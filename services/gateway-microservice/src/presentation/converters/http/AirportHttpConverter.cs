using presentation.dto.http;
using presentation.dto.http.Airport;

namespace presentation.converters.http;

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
        if (airport == null) return null!;
        
        return new AirportDTO
        {
            Id = airport.Id,
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country,
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
            Country = dto.Country,
        };
    }

    /// <summary>
    /// Converts domain Airport to UpdateAirportDTO
    /// </summary>
    public static UpdateAirportDTO ToUpdateDTO(core.domain.Airport airport)
    {
        return new UpdateAirportDTO
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
        if (dto == null || existingAirport == null) return existingAirport;
        
        existingAirport.Name = dto.Name ?? existingAirport.Name;
        existingAirport.City = dto.City ?? existingAirport.City;
        existingAirport.Country = dto.Country ?? existingAirport.Country;

        return existingAirport;
    }
}
