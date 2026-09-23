using core.exceptions;

namespace core.exceptions.businesslogic.services;

/// <summary>
/// Exception thrown when Privilege is not found in service operations
/// </summary>
public class PrivilegeNotFoundException : ServiceEntityNotFoundException
{
    public int PrivilegeId { get; }

    public PrivilegeNotFoundException(int privilegeId) 
        : base("Privilege", privilegeId)
    {
        PrivilegeId = privilegeId;
    }

    public PrivilegeNotFoundException(int privilegeId, Exception innerException) 
        : base("Privilege", privilegeId, innerException)
    {
        PrivilegeId = privilegeId;
    }
}

/// <summary>
/// Exception thrown when Privilege validation fails
/// </summary>
public class PrivilegeValidationException : ValidationException
{
    public PrivilegeValidationException() : base("Privilege validation failed") { }

    public PrivilegeValidationException(string message) : base(message) { }

    public PrivilegeValidationException(string message, Dictionary<string, string[]> errors) 
        : base(message, errors) { }

    public PrivilegeValidationException(Exception innerException) 
        : base("Privilege validation failed", innerException) { }
}

/// <summary>
/// Exception thrown when Privilege business rule is violated
/// </summary>
public class PrivilegeBusinessRuleViolationException : BusinessRuleViolationException
{
    public PrivilegeBusinessRuleViolationException(string ruleName) : base(ruleName) { }

    public PrivilegeBusinessRuleViolationException(string ruleName, string message) 
        : base(ruleName, message) { }

    public PrivilegeBusinessRuleViolationException(string ruleName, Exception innerException) 
        : base(ruleName, innerException) { }
}
