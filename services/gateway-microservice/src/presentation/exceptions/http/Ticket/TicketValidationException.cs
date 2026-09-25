namespace presentation.exceptions.http.Ticket;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when Ticket validation fails
/// </summary>
public class TicketValidationException : BaseHttpException
{
    /// <summary>
    /// Constructor with error data
    /// </summary>
    /// <param name="errorData">Dictionary of validation errors</param>
    public TicketValidationException(Dictionary<string, string[]> errorData)
        : base(400, "TICKET_VALIDATION_FAILED", "Ticket validation failed", errorData)
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Validation error message</param>
    public TicketValidationException(string message)
        : base(400, "TICKET_VALIDATION_FAILED", message)
    {
    }

    /// <summary>
    /// Constructor with message and error data
    /// </summary>
    /// <param name="message">Validation error message</param>
    /// <param name="errorData">Dictionary of validation errors</param>
    public TicketValidationException(string message, Dictionary<string, string[]> errorData)
        : base(400, "TICKET_VALIDATION_FAILED", message, errorData)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Validation error message</param>
    /// <param name="innerException">Inner exception</param>
    public TicketValidationException(string message, Exception innerException)
        : base(400, "TICKET_VALIDATION_FAILED", message, innerException)
    {
    }
}
