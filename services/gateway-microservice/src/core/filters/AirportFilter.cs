namespace core.filters;

/// <summary>
/// Filter criteria for querying Airports from repository
/// Used for filtering and searching airports in data access layer
/// </summary>
public class AirportFilter
{
    /// <summary>
    /// Filter by airport name (partial match, case-insensitive)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Filter by city (partial match, case-insensitive)
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Filter by country (partial match, case-insensitive)
    /// </summary>
    public string? Country { get; set; }
}
