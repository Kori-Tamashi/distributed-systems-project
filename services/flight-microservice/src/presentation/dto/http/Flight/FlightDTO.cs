using System;

namespace presentation.dto.http.Flight;

using presentation.dto.http.Airport;

/// <summary>
/// DTO for reading Flight data (full representation)
/// </summary>
public class FlightDTO : BaseHttpDTO
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
    /// Departure airport information (navigation property)
    /// </summary>
    public AirportDTO? FromAirport { get; set; }

    /// <summary>
    /// Arrival airport information (navigation property)
    /// </summary>
    public AirportDTO? ToAirport { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public FlightDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public FlightDTO(
        int id,
        string flightNumber,
        Guid flightUid,
        DateTime dateTime,
        int fromAirportId,
        int toAirportId,
        int price,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
        : base(id)
    {
        Id = id;
        FlightNumber = flightNumber;
        FlightUid = flightUid;
        DateTime = dateTime;
        FromAirportId = fromAirportId;
        ToAirportId = toAirportId;
        Price = price;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
