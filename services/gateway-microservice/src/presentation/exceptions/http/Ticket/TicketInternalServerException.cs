namespace presentation.exceptions.http.Ticket;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when an internal server error occurs while processing Ticket request
/// </summary>
public class TicketInternalServerException : BaseHttpException
{
    /// <summary>
    /// Constructor with inner exception
    /// </summary>
    /// <param name="innerException">Inner exception</param>
    public TicketInternalServerException(Exception innerException)
        : base(500, "TICKET_INTERNAL_ERROR", "An internal error occurred while processing Ticket request", innerException)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public TicketInternalServerException(string message, Exception innerException)
        : base(500, "TICKET_INTERNAL_ERROR", message, innerException)
    {
    }
}
