using core.enums;

using System;

namespace presentation.dto.http.Ticket;

/// <summary>
/// DTO for reading Ticket data (per lab2-template v1 spec)
/// </summary>
public class TicketDTO
{
    /// <summary>
    /// Unique ticket UID (UUID)
    /// </summary>
    public Guid TicketUid { get; set; }

    /// <summary>
    /// Primary key (database-generated)
    /// </summary>
    public int Id { get; set; }

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
    public TicketStatus Status { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public TicketDTO()
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public TicketDTO(
        int id,
        Guid ticketUid,
        string username,
        string flightNumber,
        int price,
        TicketStatus status)
    {
        Id = id;
        TicketUid = ticketUid;
        Username = username;
        FlightNumber = flightNumber;
        Price = price;
        Status = status;
    }
}
