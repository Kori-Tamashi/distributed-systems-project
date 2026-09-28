using core.exceptions;

namespace core.exceptions.businesslogic.services;

/// <summary>
/// Base exception for business logic services
/// </summary>
public abstract class BaseServiceException : BaseException
{
    protected BaseServiceException() : base()
    {
    }

    protected BaseServiceException(string message) : base(message)
    {
    }

    protected BaseServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : BaseServiceException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException() : base()
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Dictionary<string, string[]> errors) 
        : base(message)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>();
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : BaseServiceException
{
    public string? RuleName { get; }

    public BusinessRuleViolationException() : base()
    {
    }

    public BusinessRuleViolationException(string message) : base(message)
    {
    }

    public BusinessRuleViolationException(string ruleName, string message) 
        : base(message)
    {
        RuleName = ruleName;
    }

    public BusinessRuleViolationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class ServiceEntityNotFoundException : BaseServiceException
{
    public string EntityName { get; }
    public int EntityId { get; }

    public ServiceEntityNotFoundException() : base()
    {
        EntityName = "Entity";
        EntityId = 0;
    }

    public ServiceEntityNotFoundException(string message) : base(message)
    {
        EntityName = "Entity";
        EntityId = 0;
    }

    public ServiceEntityNotFoundException(string entityName, int entityId) 
        : base($"{entityName} with id {entityId} not found")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public ServiceEntityNotFoundException(string entityName, int entityId, Exception innerException) 
        : base($"{entityName} with id {entityId} not found", innerException)
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public ServiceEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
        EntityName = "Entity";
        EntityId = 0;
    }
}
