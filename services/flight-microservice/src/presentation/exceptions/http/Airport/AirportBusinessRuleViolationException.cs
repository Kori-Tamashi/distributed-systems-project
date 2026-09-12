using System.Net;

namespace presentation.exceptions.http;

/// <summary>
/// Exception thrown when an Airport business rule is violated
/// HTTP Status: 409 Conflict
/// </summary>
public class AirportBusinessRuleViolationException : BaseHttpException
{
    /// <summary>
    /// Type of business rule violation
    /// </summary>
    public string RuleType { get; }

    /// <summary>
    /// Constructor with rule type and message
    /// </summary>
    /// <param name="ruleType">Type of business rule violated</param>
    /// <param name="message">Error message</param>
    public AirportBusinessRuleViolationException(string ruleType, string message)
        : base((int)HttpStatusCode.Conflict, "AIRPORT_BUSINESS_RULE_VIOLATION", message)
    {
        RuleType = ruleType;
    }

    /// <summary>
    /// Constructor with rule type, message and error data
    /// </summary>
    /// <param name="ruleType">Type of business rule violated</param>
    /// <param name="message">Error message</param>
    /// <param name="errorData">Additional error data</param>
    public AirportBusinessRuleViolationException(string ruleType, string message, Dictionary<string, string[]> errorData)
        : base((int)HttpStatusCode.Conflict, "AIRPORT_BUSINESS_RULE_VIOLATION", message, errorData)
    {
        RuleType = ruleType;
    }

    /// <summary>
    /// Constructor for unique constraint violation
    /// </summary>
    /// <param name="fieldName">Field name with duplicate value</param>
    /// <param name="value">Duplicate value</param>
    public AirportBusinessRuleViolationException(string fieldName, object value)
        : base((int)HttpStatusCode.Conflict, "AIRPORT_UNIQUE_CONSTRAINT", $"Airport with {fieldName} '{value}' already exists")
    {
        RuleType = "UniqueConstraint";
        ErrorData = new Dictionary<string, string[]>
        {
            [fieldName] = new[] { $"Value '{value}' already exists" }
        };
    }
}
