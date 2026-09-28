namespace presentation.dto.http.Flight;

/// <summary>
/// DTO for updating Flight
/// </summary>
public class UpdateFlightDTO
{
    /// <summary>
    /// Flight number (optional)
    /// </summary>
    public string? FlightNumber { get; set; }

    /// <summary>
    /// Flight date and time (optional)
    /// </summary>
    public DateTime? DateTime { get; set; }

    /// <summary>
    /// Departure airport ID (optional)
    /// </summary>
    public int? FromAirportId { get; set; }

    /// <summary>
    /// Arrival airport ID (optional)
    /// </summary>
    public int? ToAirportId { get; set; }

    /// <summary>
    /// Flight price in rubles (optional)
    /// </summary>
    public int? Price { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdateFlightDTO()
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public UpdateFlightDTO(string? flightNumber = null, DateTime? dateTime = null, int? fromAirportId = null, int? toAirportId = null, int? price = null)
    {
        FlightNumber = flightNumber;
        DateTime = dateTime;
        FromAirportId = fromAirportId;
        ToAirportId = toAirportId;
        Price = price;
    }
}
