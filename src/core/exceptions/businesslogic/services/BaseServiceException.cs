namespace core.exceptions.businesslogic.services;

/// <summary>
/// Base exception for all business logic service operations
/// </summary>
public class BaseServiceException : Exception
{
    public BaseServiceException() : base() { }

    public BaseServiceException(string message) : base(message) { }

    public BaseServiceException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Base exception for validation errors in service layer
/// </summary>
public class ValidationException : BaseServiceException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException() : base("Validation failed")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Dictionary<string, string[]> errors) : base(message)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Add a validation error for a specific field
    /// </summary>
    public void AddError(string propertyName, string errorMessage)
    {
        if (Errors.ContainsKey(propertyName))
        {
            var errors = Errors[propertyName].ToList();
            errors.Add(errorMessage);
            Errors[propertyName] = errors.ToArray();
        }
        else
        {
            Errors[propertyName] = new[] { errorMessage };
        }
    }
}

/// <summary>
/// Base exception for business rule violations
/// </summary>
public class BusinessRuleViolationException : BaseServiceException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName) 
        : base($"Business rule '{ruleName}' was violated")
    {
        RuleName = ruleName;
    }

    public BusinessRuleViolationException(string ruleName, string message) : base(message)
    {
        RuleName = ruleName;
    }

    public BusinessRuleViolationException(string ruleName, Exception innerException) 
        : base($"Business rule '{ruleName}' was violated", innerException)
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Base exception for entity not found in service operations
/// </summary>
public class ServiceEntityNotFoundException : BaseServiceException
{
    public string EntityName { get; }
    public object? Id { get; }

    public ServiceEntityNotFoundException(string entityName, object? id) 
        : base($"{entityName} with id {id} was not found")
    {
        EntityName = entityName;
        Id = id;
    }

    public ServiceEntityNotFoundException(string entityName, object? id, Exception innerException) 
        : base($"{entityName} with id {id} was not found", innerException)
    {
        EntityName = entityName;
        Id = id;
    }
}
