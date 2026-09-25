using core.enums;

using System;

namespace presentation.dto.http.Ticket;

/// <summary>
/// DTO for creating a new Ticket (per lab2-template v1 spec)
/// </summary>
public class CreateTicketDTO
{
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
    /// Ticket status (PAID, CANCELED)
    /// </summary>
    public TicketStatus Status { get; set; } = TicketStatus.Paid;
}
