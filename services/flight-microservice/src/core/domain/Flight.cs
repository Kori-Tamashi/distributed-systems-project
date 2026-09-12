namespace core.domain;

/// <summary>
/// Domain entity representing a Flight
/// </summary>
public class Flight
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

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
    /// Navigation property - departure airport
    /// </summary>
    public Airport? FromAirport { get; set; }

    /// <summary>
    /// Navigation property - arrival airport
    /// </summary>
    public Airport? ToAirport { get; set; }
}
