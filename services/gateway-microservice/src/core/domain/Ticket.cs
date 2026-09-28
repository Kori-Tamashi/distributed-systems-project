namespace core.domain;

/// <summary>
/// Domain entity representing a Ticket (per lab2-template v1 spec)
/// Table: ticket
/// Columns: id, ticket_uid, username, flight_number, price, status
/// </summary>
public class Ticket
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique ticket UID (UUID)
    /// </summary>
    public Guid TicketUid { get; set; }

    /// <summary>
    /// Username of the ticket owner
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Flight number (e.g., "AFL031")
    /// </summary>
    public string FlightNumber { get; set; } = string.Empty;

    /// <summary>
    /// Ticket price in rubles
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// Ticket status: PAID (1) or CANCELED (2)
    /// </summary>
    public int Status { get; set; }
}
