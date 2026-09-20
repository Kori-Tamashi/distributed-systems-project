namespace presentation.exceptions.http.Ticket;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Ticket business rule is violated
/// </summary>
public class TicketBusinessRuleViolationException : BaseHttpException
{
    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Business rule violation message</param>
    public TicketBusinessRuleViolationException(string message)
        : base(409, "TICKET_BUSINESS_RULE_VIOLATION", message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Business rule violation message</param>
    /// <param name="innerException">Inner exception</param>
    public TicketBusinessRuleViolationException(string message, Exception innerException)
        : base(409, "TICKET_BUSINESS_RULE_VIOLATION", message, innerException)
    {
    }
}
