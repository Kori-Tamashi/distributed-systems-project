using core.domain;

using core.enums;

namespace core.filters;

/// <summary>
/// Filter criteria for querying Tickets from repository
/// Used for filtering and searching tickets in data access layer
/// </summary>
public class TicketFilter
{
    /// <summary>
    /// Filter by flight ID
    /// </summary>
    public int? FlightId { get; set; }

    /// <summary>
    /// Filter by passenger name (partial match, case-insensitive)
    /// </summary>
    public string? PassengerName { get; set; }

    /// <summary>
    /// Filter by passenger email (partial match, case-insensitive)
    /// </summary>
    public string? PassengerEmail { get; set; }

    /// <summary>
    /// Filter by seat number (partial match, case-insensitive)
    /// </summary>
    public string? SeatNumber { get; set; }

    /// <summary>
    /// Filter by ticket class
    /// </summary>
    public TicketClass? Class { get; set; }

    /// <summary>
    /// Filter by minimum price
    /// </summary>
    public int? MinPrice { get; set; }

    /// <summary>
    /// Filter by maximum price
    /// </summary>
    public int? MaxPrice { get; set; }

    /// <summary>
    /// Filter by minimum booking date
    /// </summary>
    public DateTime? MinBookingDate { get; set; }

    /// <summary>
    /// Filter by maximum booking date
    /// </summary>
    public DateTime? MaxBookingDate { get; set; }

    /// <summary>
    /// Filter by ticket status
    /// </summary>
    public TicketStatus? Status { get; set; }
}
