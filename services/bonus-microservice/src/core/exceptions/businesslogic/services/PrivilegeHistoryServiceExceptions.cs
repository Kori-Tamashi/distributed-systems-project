using core.exceptions;

namespace core.exceptions.businesslogic.services;

/// <summary>
/// Exception thrown when PrivilegeHistory is not found in service operations
/// </summary>
public class PrivilegeHistoryNotFoundException : ServiceEntityNotFoundException
{
    public int HistoryId { get; }

    public PrivilegeHistoryNotFoundException(int historyId) 
        : base("PrivilegeHistory", historyId)
    {
        HistoryId = historyId;
    }

    public PrivilegeHistoryNotFoundException(int historyId, Exception innerException) 
        : base("PrivilegeHistory", historyId, innerException)
    {
        HistoryId = historyId;
    }
}

/// <summary>
/// Exception thrown when PrivilegeHistory validation fails
/// </summary>
public class PrivilegeHistoryValidationException : ValidationException
{
    public PrivilegeHistoryValidationException() : base("PrivilegeHistory validation failed") { }

    public PrivilegeHistoryValidationException(string message) : base(message) { }

    public PrivilegeHistoryValidationException(string message, Dictionary<string, string[]> errors) 
        : base(message, errors) { }

    public PrivilegeHistoryValidationException(Exception innerException) 
        : base("PrivilegeHistory validation failed", innerException) { }
}

/// <summary>
/// Exception thrown when PrivilegeHistory business rule is violated
/// </summary>
public class PrivilegeHistoryBusinessRuleViolationException : BusinessRuleViolationException
{
    public PrivilegeHistoryBusinessRuleViolationException(string ruleName) : base(ruleName) { }

    public PrivilegeHistoryBusinessRuleViolationException(string ruleName, string message) 
        : base(ruleName, message) { }

    public PrivilegeHistoryBusinessRuleViolationException(string ruleName, Exception innerException) 
        : base(ruleName, innerException) { }
}
