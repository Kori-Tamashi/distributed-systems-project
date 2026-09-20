using presentation.dto.http;
using presentation.dto.http.Flight;

namespace presentation.converters.http;

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
        if (flight == null) return null!;
        
        return new FlightDTO
        {
            Id = flight.Id,
            FlightUid = flight.FlightUid,
            FlightNumber = flight.FlightNumber,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = flight.Price,
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
            FlightUid = dto.FlightUid,
            FlightNumber = dto.FlightNumber,
            DateTime = dto.DateTime,
            FromAirportId = dto.FromAirportId,
            ToAirportId = dto.ToAirportId,
            Price = dto.Price
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
    /// Converts list of FlightDTOs to list of domain Flights
    /// </summary>
    public static List<core.domain.Flight> ToDomain(List<FlightDTO> dtos)
    {
        return dtos.Select(ToDomain).ToList();
    }

    /// <summary>
    /// Converts domain Flight to CreateFlightDTO
    /// </summary>
    public static CreateFlightDTO ToCreateDTO(core.domain.Flight flight)
    {
        return new CreateFlightDTO
        {
            FlightNumber = flight.FlightNumber,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = flight.Price
        };
    }

    /// <summary>
    /// Converts CreateFlightDTO to domain Flight
    /// </summary>
    public static core.domain.Flight ToCreateDomain(CreateFlightDTO dto)
    {
        return new core.domain.Flight
        {
            Id = 0,
            FlightNumber = dto.FlightNumber,
            DateTime = dto.DateTime,
            FromAirportId = dto.FromAirportId,
            ToAirportId = dto.ToAirportId,
            Price = dto.Price,
        };
    }

    /// <summary>
    /// Converts domain Flight to UpdateFlightDTO
    /// </summary>
    public static UpdateFlightDTO ToUpdateDTO(core.domain.Flight flight)
    {
        return new UpdateFlightDTO
        {
            FlightNumber = flight.FlightNumber,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = flight.Price
        };
    }

    /// <summary>
    /// Converts UpdateFlightDTO to domain Flight
    /// Only updates non-null properties
    /// </summary>
    public static core.domain.Flight ToUpdateDomain(UpdateFlightDTO dto, core.domain.Flight existingFlight)
    {
        if (dto == null || existingFlight == null) return existingFlight;
        
        existingFlight.FlightNumber = dto.FlightNumber ?? existingFlight.FlightNumber;
        existingFlight.DateTime = dto.DateTime ?? existingFlight.DateTime;
        existingFlight.FromAirportId = dto.FromAirportId ?? existingFlight.FromAirportId;
        existingFlight.ToAirportId = dto.ToAirportId ?? existingFlight.ToAirportId;
        existingFlight.Price = dto.Price ?? existingFlight.Price;

        return existingFlight;
    }
}
