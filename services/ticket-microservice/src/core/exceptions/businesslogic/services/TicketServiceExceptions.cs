using core.exceptions;

namespace core.exceptions.businesslogic.services;

/// <summary>
/// Exception thrown when Ticket is not found in service operations
/// </summary>
public class TicketNotFoundException : ServiceEntityNotFoundException
{
    public int TicketId { get; }

    public TicketNotFoundException(int ticketId) 
        : base("Ticket", ticketId)
    {
        TicketId = ticketId;
    }

    public TicketNotFoundException(int ticketId, Exception innerException) 
        : base("Ticket", ticketId, innerException)
    {
        TicketId = ticketId;
    }
}

/// <summary>
/// Exception thrown when Ticket validation fails
/// </summary>
public class TicketValidationException : ValidationException
{
    public TicketValidationException() : base("Ticket validation failed") { }

    public TicketValidationException(string message) : base(message) { }

    public TicketValidationException(string message, Dictionary<string, string[]> errors) 
        : base(message, errors) { }

    public TicketValidationException(Exception innerException) 
        : base("Ticket validation failed", innerException) { }
}

/// <summary>
/// Exception thrown when Ticket business rule is violated
/// </summary>
public class TicketBusinessRuleViolationException : BusinessRuleViolationException
{
    public TicketBusinessRuleViolationException(string ruleName) : base(ruleName) { }

    public TicketBusinessRuleViolationException(string ruleName, string message) 
        : base(ruleName, message) { }

    public TicketBusinessRuleViolationException(string ruleName, Exception innerException) 
        : base(ruleName, innerException) { }
}
