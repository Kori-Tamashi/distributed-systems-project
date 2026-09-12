using core.exceptions;

namespace core.exceptions.businesslogic.services;

/// <summary>
/// Exception thrown when Airport is not found in service operations
/// </summary>
public class AirportNotFoundException : ServiceEntityNotFoundException
{
    public int AirportId { get; }

    public AirportNotFoundException(int airportId) 
        : base("Airport", airportId)
    {
        AirportId = airportId;
    }

    public AirportNotFoundException(int airportId, Exception innerException) 
        : base("Airport", airportId, innerException)
    {
        AirportId = airportId;
    }
}

/// <summary>
/// Exception thrown when Airport validation fails
/// </summary>
public class AirportValidationException : ValidationException
{
    public AirportValidationException() : base("Airport validation failed") { }

    public AirportValidationException(string message) : base(message) { }

    public AirportValidationException(string message, Dictionary<string, string[]> errors) 
        : base(message, errors) { }

    public AirportValidationException(Exception innerException) 
        : base("Airport validation failed", innerException) { }
}

/// <summary>
/// Exception thrown when Airport business rule is violated
/// </summary>
public class AirportBusinessRuleViolationException : BusinessRuleViolationException
{
    public AirportBusinessRuleViolationException(string ruleName) : base(ruleName) { }

    public AirportBusinessRuleViolationException(string ruleName, string message) 
        : base(ruleName, message) { }

    public AirportBusinessRuleViolationException(string ruleName, Exception innerException) 
        : base(ruleName, innerException) { }
}
