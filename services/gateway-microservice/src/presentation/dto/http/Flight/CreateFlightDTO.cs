namespace presentation.dto.http.Flight;

/// <summary>
/// DTO for creating Flight
/// </summary>
public class CreateFlightDTO
{
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
    /// Unique flight identifier (auto-generated if not provided)
    /// </summary>
    public Guid FlightUid { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CreateFlightDTO()
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public CreateFlightDTO(string flightNumber, DateTime dateTime, int fromAirportId, int toAirportId, int price)
    {
        FlightNumber = flightNumber;
        DateTime = dateTime;
        FromAirportId = fromAirportId;
        ToAirportId = toAirportId;
        Price = price;
    }
}
