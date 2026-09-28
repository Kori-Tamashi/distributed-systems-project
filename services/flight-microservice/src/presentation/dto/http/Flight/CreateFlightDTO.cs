using System;

namespace presentation.dto.http;

/// <summary>
/// DTO for creating a new Flight
/// </summary>
public class CreateFlightDTO : BaseHttpDTO
{
    /// <summary>
    /// Flight number (e.g., "SU100", "AF200")
    /// </summary>
    public string FlightNumber { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the flight (GUID)
    /// </summary>
    public Guid FlightUid { get; set; }

    /// <summary>
    /// Scheduled departure date and time (UTC)
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Departure airport ID
    /// </summary>
    public int FromAirportId { get; set; }

    /// <summary>
    /// Arrival airport ID
    /// </summary>
    public int ToAirportId { get; set; }

    /// <summary>
    /// Flight price in rubles
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CreateFlightDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all required fields
    /// </summary>
    public CreateFlightDTO(
        string flightNumber,
        Guid flightUid,
        DateTime dateTime,
        int fromAirportId,
        int toAirportId,
        int price)
        : base(0)
    {
        FlightNumber = flightNumber;
        FlightUid = flightUid;
        DateTime = dateTime;
        FromAirportId = fromAirportId;
        ToAirportId = toAirportId;
        Price = price;
    }
}
