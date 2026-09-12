using presentation.dto.http;
using presentation.dto.http.Airport;
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
        return new FlightDTO
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            FlightUid = flight.FlightUid,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = flight.Price,
            FromAirport = null, // Will be populated by controller if needed
            ToAirport = null    // Will be populated by controller if needed
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
            FlightUid = flight.FlightUid,
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
            FlightUid = dto.FlightUid,
            DateTime = dto.DateTime,
            FromAirportId = dto.FromAirportId,
            ToAirportId = dto.ToAirportId,
            Price = dto.Price
        };
    }

    /// <summary>
    /// Converts domain Flight to UpdateFlightDTO
    /// </summary>
    public static UpdateFlightDTO ToUpdateDTO(core.domain.Flight flight)
    {
        return new UpdateFlightDTO(flight.Id)
        {
            FlightNumber = flight.FlightNumber,
            FlightUid = flight.FlightUid,
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
        existingFlight.FlightNumber = dto.FlightNumber ?? existingFlight.FlightNumber;
        existingFlight.FlightUid = dto.FlightUid ?? existingFlight.FlightUid;
        existingFlight.DateTime = dto.DateTime ?? existingFlight.DateTime;
        existingFlight.FromAirportId = dto.FromAirportId ?? existingFlight.FromAirportId;
        existingFlight.ToAirportId = dto.ToAirportId ?? existingFlight.ToAirportId;
        existingFlight.Price = dto.Price ?? existingFlight.Price;

        return existingFlight;
    }

    /// <summary>
    /// Converts domain Flight to UpdateFlightDTO (without existing flight)
    /// </summary>
    public static UpdateFlightDTO ToUpdateDTOWithoutMerge(core.domain.Flight flight)
    {
        return new UpdateFlightDTO(flight.Id)
        {
            FlightNumber = flight.FlightNumber,
            FlightUid = flight.FlightUid,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = flight.Price
        };
    }

    /// <summary>
    /// Converts domain Flight to UpdateFlightDTO with only changed properties
    /// </summary>
    public static UpdateFlightDTO ToUpdateDTOPartial(core.domain.Flight flight, params string[] changedProperties)
    {
        var dto = new UpdateFlightDTO(flight.Id);

        if (Array.Exists(changedProperties, p => p.Equals("FlightNumber", StringComparison.OrdinalIgnoreCase)))
            dto.FlightNumber = flight.FlightNumber;

        if (Array.Exists(changedProperties, p => p.Equals("FlightUid", StringComparison.OrdinalIgnoreCase)))
            dto.FlightUid = flight.FlightUid;

        if (Array.Exists(changedProperties, p => p.Equals("DateTime", StringComparison.OrdinalIgnoreCase)))
            dto.DateTime = flight.DateTime;

        if (Array.Exists(changedProperties, p => p.Equals("FromAirportId", StringComparison.OrdinalIgnoreCase)))
            dto.FromAirportId = flight.FromAirportId;

        if (Array.Exists(changedProperties, p => p.Equals("ToAirportId", StringComparison.OrdinalIgnoreCase)))
            dto.ToAirportId = flight.ToAirportId;

        if (Array.Exists(changedProperties, p => p.Equals("Price", StringComparison.OrdinalIgnoreCase)))
            dto.Price = flight.Price;

        return dto;
    }

    /// <summary>
    /// Converts domain Flight to UpdateFlightDTO with only changed properties (nullable check)
    /// </summary>
    public static UpdateFlightDTO ToUpdateDTOPartialNullable(core.domain.Flight flight, params bool[] propertyChanged)
    {
        var dto = new UpdateFlightDTO(flight.Id);

        if (propertyChanged.Length >= 1 && propertyChanged[0])
            dto.FlightNumber = flight.FlightNumber;

        if (propertyChanged.Length >= 2 && propertyChanged[1])
            dto.FlightUid = flight.FlightUid;

        if (propertyChanged.Length >= 3 && propertyChanged[2])
            dto.DateTime = flight.DateTime;

        if (propertyChanged.Length >= 4 && propertyChanged[3])
            dto.FromAirportId = flight.FromAirportId;

        if (propertyChanged.Length >= 5 && propertyChanged[4])
            dto.ToAirportId = flight.ToAirportId;

        if (propertyChanged.Length >= 6 && propertyChanged[5])
            dto.Price = flight.Price;

        return dto;
    }
}
