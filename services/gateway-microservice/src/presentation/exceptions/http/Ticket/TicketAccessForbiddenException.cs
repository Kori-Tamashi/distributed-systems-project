namespace presentation.exceptions.http.Ticket;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Ticket access is forbidden (user does not own the ticket)
/// </summary>
public class TicketAccessForbiddenException : BaseHttpException
{
    /// <summary>
    /// The UID of the Ticket that access was forbidden for
    /// </summary>
    public Guid TicketUid { get; }

    /// <summary>
    /// Constructor with Ticket UID
    /// </summary>
    /// <param name="ticketUid">The UID of the Ticket</param>
    public TicketAccessForbiddenException(Guid ticketUid)
        : base(403, "TICKET_ACCESS_FORBIDDEN", $"Ticket with UID {ticketUid} does not belong to user")
    {
        TicketUid = ticketUid;
    }

    public TicketAccessForbiddenException(Guid ticketUid, Exception innerException)
        : base(403, "TICKET_ACCESS_FORBIDDEN", $"Ticket with UID {ticketUid} does not belong to user", innerException)
    {
        TicketUid = ticketUid;
    }
}
