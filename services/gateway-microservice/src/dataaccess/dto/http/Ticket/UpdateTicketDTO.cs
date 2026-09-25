using System;
using dataaccess.dto.http;

namespace dataaccess.dto.http.Ticket;

/// <summary>
/// DTO for updating Ticket (per lab2-template v1 spec)
/// </summary>
public class UpdateTicketDTO : BaseHttpDTO
{
    /// <summary>
    /// Unique ticket UID (UUID)
    /// </summary>
    public Guid TicketUid { get; set; }

    /// <summary>
    /// Username of the ticket owner (optional)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Flight number (e.g., "AFL031") (optional)
    /// </summary>
    public string? FlightNumber { get; set; }

    /// <summary>
    /// Ticket price in rubles (optional)
    /// </summary>
    public int? Price { get; set; }

    /// <summary>
    /// Ticket status: PAID (1) or CANCELED (2) (optional)
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdateTicketDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public UpdateTicketDTO(
        Guid ticketUid,
        string? username = null,
        string? flightNumber = null,
        int? price = null,
        int? status = null)
        : base(0)
    {
        TicketUid = ticketUid;
        Username = username;
        FlightNumber = flightNumber;
        Price = price;
        Status = status;
    }
}
