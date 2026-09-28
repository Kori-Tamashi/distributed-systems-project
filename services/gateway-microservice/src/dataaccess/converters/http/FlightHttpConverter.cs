using dataaccess.dto.http;
using dataaccess.dto.http.Flight;
using dataaccess.dto.http.Airport;

namespace dataaccess.converters.http;

/// <summary>
/// Converter for Flight between domain and HTTP DTO representations
/// </summary>
public static class FlightHttpConverter
{
    /// <summary>
    /// Converts domain Flight to FlightDTO
    /// </summary>
    public static FlightDTO ToDTO(core.domain.Flight flight)
    {
        return new FlightDTO
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            FlightUid = flight.FlightUid,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = flight.Price,
            FromAirport = flight.FromAirport != null ? new AirportDTO
            {
                Id = flight.FromAirport.Id,
                Name = flight.FromAirport.Name,
                City = flight.FromAirport.City,
                Country = flight.FromAirport.Country
            } : null,
            ToAirport = flight.ToAirport != null ? new AirportDTO
            {
                Id = flight.ToAirport.Id,
                Name = flight.ToAirport.Name,
                City = flight.ToAirport.City,
                Country = flight.ToAirport.Country
            } : null
        };
    }

    /// <summary>
    /// Converts FlightDTO to domain Flight
    /// </summary>
    public static core.domain.Flight ToDomain(FlightDTO dto)
    {
        return new core.domain.Flight
        {
            Id = dto.Id,
            FlightNumber = dto.FlightNumber,
            FlightUid = dto.FlightUid,
            DateTime = dto.DateTime,
            FromAirportId = dto.FromAirportId,
            ToAirportId = dto.ToAirportId,
            Price = dto.Price,
            FromAirport = dto.FromAirport != null ? new core.domain.Airport
            {
                Id = dto.FromAirport.Id,
                Name = dto.FromAirport.Name,
                City = dto.FromAirport.City,
                Country = dto.FromAirport.Country
            } : null,
            ToAirport = dto.ToAirport != null ? new core.domain.Airport
            {
                Id = dto.ToAirport.Id,
                Name = dto.ToAirport.Name,
                City = dto.ToAirport.City,
                Country = dto.ToAirport.Country
            } : null
        };
    }

    /// <summary>
    /// Converts CreateFlightDTO to domain Flight
    /// </summary>
    public static core.domain.Flight ToDomain(CreateFlightDTO dto)
    {
        return new core.domain.Flight
        {
            Id = dto.Id,
            FlightNumber = dto.FlightNumber,
            FlightUid = dto.FlightUid == Guid.Empty ? Guid.NewGuid() : dto.FlightUid,
            DateTime = dto.DateTime,
            FromAirportId = dto.FromAirportId,
            ToAirportId = dto.ToAirportId,
            Price = dto.Price
        };
    }

    /// <summary>
    /// Converts UpdateFlightDTO to domain Flight
    /// </summary>
    public static core.domain.Flight ToDomain(UpdateFlightDTO dto)
    {
        return new core.domain.Flight
        {
            Id = dto.Id,
            FlightNumber = dto.FlightNumber ?? string.Empty,
            FlightUid = dto.FlightUid ?? Guid.Empty,
            DateTime = dto.DateTime ?? DateTime.MinValue,
            FromAirportId = dto.FromAirportId ?? 0,
            ToAirportId = dto.ToAirportId ?? 0,
            Price = dto.Price ?? 0
        };
    }

    /// <summary>
    /// Converts domain Flight to UpdateFlightDTO
    /// </summary>
    public static UpdateFlightDTO ToUpdateDTO(core.domain.Flight flight)
    {
        return new UpdateFlightDTO
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            FlightUid = flight.FlightUid,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = flight.Price
        };
    }

    /// <summary>
    /// Converts list of domain Flights to list of FlightDTOs
    /// </summary>
    public static List<FlightDTO> ToDTO(List<core.domain.Flight> flights)
    {
        return flights.Select(ToDTO).ToList();
    }

    /// <summary>
    /// Converts IEnumerable of domain Flights to IEnumerable of FlightDTOs
    /// </summary>
    public static IEnumerable<FlightDTO> ToDTO(IEnumerable<core.domain.Flight> flights)
    {
        return flights.Select(ToDTO);
    }
}
