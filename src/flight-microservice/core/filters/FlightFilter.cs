namespace core.filters;

/// <summary>
/// Filter criteria for querying Flights from repository
/// Used for filtering and searching flights in data access layer
/// </summary>
public class FlightFilter
{
    /// <summary>
    /// Filter by flight number (partial match, case-insensitive)
    /// </summary>
    public string? FlightNumber { get; set; }

    /// <summary>
    /// Filter by departure airport ID
    /// </summary>
    public int? FromAirportId { get; set; }

    /// <summary>
    /// Filter by arrival airport ID
    /// </summary>
    public int? ToAirportId { get; set; }

    /// <summary>
    /// Filter by minimum price
    /// </summary>
    public int? MinPrice { get; set; }

    /// <summary>
    /// Filter by maximum price
    /// </summary>
    public int? MaxPrice { get; set; }

    /// <summary>
    /// Filter by minimum flight date
    /// </summary>
    public DateTime? MinDateTime { get; set; }

    /// <summary>
    /// Filter by maximum flight date
    /// </summary>
    public DateTime? MaxDateTime { get; set; }
}
