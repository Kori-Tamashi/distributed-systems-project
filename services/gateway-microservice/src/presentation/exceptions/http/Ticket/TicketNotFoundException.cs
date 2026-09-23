namespace presentation.exceptions.http.Ticket;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Ticket is not found
/// </summary>
public class TicketNotFoundException : BaseHttpException
{
    /// <summary>
    /// The ID of the Ticket that was not found
    /// </summary>
    public int TicketId { get; }

    /// <summary>
    /// Constructor with Ticket ID
    /// </summary>
    /// <param name="ticketId">The ID of the Ticket that was not found</param>
    public TicketNotFoundException(int ticketId)
        : base(404, "TICKET_NOT_FOUND", $"Ticket with ID {ticketId} not found")
    {
        TicketId = ticketId;
    }

    /// <summary>
    /// Constructor with Ticket ID and inner exception
    /// </summary>
    /// <param name="ticketId">The ID of the Ticket that was not found</param>
    /// <param name="innerException">Inner exception</param>
    public TicketNotFoundException(int ticketId, Exception innerException)
        : base(404, "TICKET_NOT_FOUND", $"Ticket with ID {ticketId} not found", innerException)
    {
        TicketId = ticketId;
    }
}
