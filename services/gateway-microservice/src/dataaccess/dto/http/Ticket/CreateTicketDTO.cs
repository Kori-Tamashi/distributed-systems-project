using System;
using dataaccess.dto.http;

namespace dataaccess.dto.http.Ticket;

/// <summary>
/// DTO for creating a new Ticket (per lab2-template v1 spec)
/// </summary>
public class CreateTicketDTO : BaseHttpDTO
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
    public CreateTicketDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public CreateTicketDTO(
        Guid ticketUid,
        string username,
        string flightNumber,
        int price,
        int status)
        : base(0)
    {
        TicketUid = ticketUid;
        Username = username;
        FlightNumber = flightNumber;
        Price = price;
        Status = status;
    }
}
