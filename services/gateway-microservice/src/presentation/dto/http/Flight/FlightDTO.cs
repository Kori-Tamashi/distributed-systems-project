using presentation.dto.http;

namespace presentation.dto.http.Flight;

/// <summary>
/// DTO for reading Flight data (full representation)
/// </summary>
public class FlightDTO : BaseHttpDTO
{
    /// <summary>
    /// Unique flight UID (UUID)
    /// </summary>
    public Guid FlightUid { get; set; }

    /// <summary>
    /// Flight number (required, max 20 chars)
    /// </summary>
    public string FlightNumber { get; set; } = string.Empty;

    /// <summary>
    /// Flight date and time
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
    public FlightDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public FlightDTO(
        int id,
        Guid flightUid,
        string flightNumber,
        DateTime dateTime,
        int fromAirportId,
        int toAirportId,
        int price)
        : base(id)
    {
        Id = id;
        FlightUid = flightUid;
        FlightNumber = flightNumber;
        DateTime = dateTime;
        FromAirportId = fromAirportId;
        ToAirportId = toAirportId;
        Price = price;
    }
}
