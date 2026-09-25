using core.domain;
using core.enums;

namespace core.filters;

/// <summary>
/// Filter criteria for querying Tickets from repository (per lab2-template v1 spec)
/// Table: ticket
/// Columns: ticket_uid, username, flight_number, price, status
/// </summary>
public class TicketFilter
{
    /// <summary>
    /// Filter by username (partial match, case-insensitive)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Filter by flight number (partial match, case-insensitive)
    /// </summary>
    public string? FlightNumber { get; set; }

    /// <summary>
    /// Filter by minimum price
    /// </summary>
    public int? MinPrice { get; set; }

    /// <summary>
    /// Filter by maximum price
    /// </summary>
    public int? MaxPrice { get; set; }

    /// <summary>
    /// Filter by ticket status
    /// </summary>
    public TicketStatus? Status { get; set; }
}
