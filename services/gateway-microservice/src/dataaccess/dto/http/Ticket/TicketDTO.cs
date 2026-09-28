using System;

namespace dataaccess.dto.http.Ticket;

/// <summary>
/// DTO for reading Ticket data (per lab2-template v1 spec)
/// Table: ticket
/// Columns: id, ticket_uid, username, flight_number, price, status
/// </summary>
public class TicketDTO : BaseHttpDTO
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
    /// Ticket status: PAID (1) or CANCELED (2)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public TicketDTO()
        : base(0)
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
        int status)
        : base(id)
    {
        Id = id;
        TicketUid = ticketUid;
        Username = username;
        FlightNumber = flightNumber;
        Price = price;
        Status = status;
    }
}
