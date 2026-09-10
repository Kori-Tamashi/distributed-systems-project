namespace core.exceptions.businesslogic.services;

/// <summary>
/// Exception thrown when Person is not found in service operations
/// </summary>
public class PersonNotFoundException : ServiceEntityNotFoundException
{
    public int PersonId { get; }

    public PersonNotFoundException(int personId) 
        : base("Person", personId)
    {
        PersonId = personId;
    }

    public PersonNotFoundException(int personId, Exception innerException) 
        : base("Person", personId, innerException)
    {
        PersonId = personId;
    }
}

/// <summary>
/// Exception thrown when Person validation fails
/// </summary>
public class PersonValidationException : ValidationException
{
    public PersonValidationException() : base("Person validation failed") { }

    public PersonValidationException(string message) : base(message) { }

    public PersonValidationException(string message, Dictionary<string, string[]> errors) 
        : base(message, errors) { }

    public PersonValidationException(Exception innerException) 
        : base("Person validation failed", innerException) { }
}

/// <summary>
/// Exception thrown when Person business rule is violated
/// </summary>
public class PersonBusinessRuleViolationException : BusinessRuleViolationException
{
    public PersonBusinessRuleViolationException(string ruleName) : base(ruleName) { }

    public PersonBusinessRuleViolationException(string ruleName, string message) 
        : base(ruleName, message) { }

    public PersonBusinessRuleViolationException(string ruleName, Exception innerException) 
        : base(ruleName, innerException) { }
}
