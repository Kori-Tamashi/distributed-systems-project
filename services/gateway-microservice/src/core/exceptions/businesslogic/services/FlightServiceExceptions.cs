using core.exceptions;

namespace core.exceptions.businesslogic.services;

/// <summary>
/// Exception thrown when Flight is not found in service operations
/// </summary>
public class FlightNotFoundException : ServiceEntityNotFoundException
{
    public int FlightId { get; }

    public FlightNotFoundException(int flightId) 
        : base("Flight", flightId)
    {
        FlightId = flightId;
    }

    public FlightNotFoundException(int flightId, Exception innerException) 
        : base("Flight", flightId, innerException)
    {
        FlightId = flightId;
    }
}

/// <summary>
/// Exception thrown when Flight validation fails
/// </summary>
public class FlightValidationException : ValidationException
{
    public FlightValidationException() : base("Flight validation failed") { }

    public FlightValidationException(string message) : base(message) { }

    public FlightValidationException(string message, Dictionary<string, string[]> errors) 
        : base(message, errors) { }

    public FlightValidationException(Exception innerException) 
        : base("Flight validation failed", innerException) { }
}

/// <summary>
/// Exception thrown when Flight business rule is violated
/// </summary>
public class FlightBusinessRuleViolationException : BusinessRuleViolationException
{
    public FlightBusinessRuleViolationException(string ruleName) : base(ruleName) { }

    public FlightBusinessRuleViolationException(string ruleName, string message) 
        : base(ruleName, message) { }

    public FlightBusinessRuleViolationException(string ruleName, Exception innerException) 
        : base(ruleName, innerException) { }
}
