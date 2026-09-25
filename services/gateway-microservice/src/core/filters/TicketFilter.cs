namespace core.filters;

/// <summary>
/// Filter criteria for querying Tickets (per lab2-template v1 spec)
/// </summary>
public class TicketFilter
{
    /// <summary>
    /// Filter by flight number (e.g., "AFL031")
    /// </summary>
    public string? FlightNumber { get; set; }

    /// <summary>
    /// Filter by username (partial match, case-insensitive)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Filter by minimum price
    /// </summary>
    public int? MinPrice { get; set; }

    /// <summary>
    /// Filter by maximum price
    /// </summary>
    public int? MaxPrice { get; set; }

    /// <summary>
    /// Filter by ticket status (PAID=1, CANCELED=2)
    /// </summary>
    public int? Status { get; set; }
}
