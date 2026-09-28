using System.Net;

namespace presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Ticket is not found
/// HTTP Status: 404 Not Found
/// </summary>
public class TicketNotFoundException : BaseHttpException
{
    /// <summary>
    /// Ticket ID that was not found
    /// </summary>
    public int TicketId { get; }

    /// <summary>
    /// Constructor with ticket ID
    /// </summary>
    /// <param name="ticketId">Ticket ID that was not found</param>
    public TicketNotFoundException(int ticketId)
        : base((int)HttpStatusCode.NotFound, "TICKET_NOT_FOUND", $"Ticket with ID {ticketId} was not found")
    {
        TicketId = ticketId;
    }

    /// <summary>
    /// Constructor with ticket ID and custom message
    /// </summary>
    /// <param name="ticketId">Ticket ID that was not found</param>
    /// <param name="message">Custom error message</param>
    public TicketNotFoundException(int ticketId, string message)
        : base((int)HttpStatusCode.NotFound, "TICKET_NOT_FOUND", message)
    {
        TicketId = ticketId;
    }
}
