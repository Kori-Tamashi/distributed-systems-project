namespace presentation.exceptions.http;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Privilege business rule is violated
/// </summary>
public class PrivilegeBusinessRuleViolationException : BaseHttpException
{
    /// <summary>
    /// Constructor with rule name and message
    /// </summary>
    /// <param name="ruleName">Name of the violated rule</param>
    /// <param name="message">Exception message</param>
    public PrivilegeBusinessRuleViolationException(string ruleName, string message)
        : base(409, "PRIVILEGE_BUSINESS_RULE_VIOLATED", message)
    {
        RuleName = ruleName;
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Exception message</param>
    public PrivilegeBusinessRuleViolationException(string message)
        : base(409, "PRIVILEGE_BUSINESS_RULE_VIOLATED", message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeBusinessRuleViolationException(string message, Exception innerException)
        : base(409, "PRIVILEGE_BUSINESS_RULE_VIOLATED", message, innerException)
    {
    }

    /// <summary>
    /// Name of the violated business rule
    /// </summary>
    public string RuleName { get; protected set; } = string.Empty;
}
