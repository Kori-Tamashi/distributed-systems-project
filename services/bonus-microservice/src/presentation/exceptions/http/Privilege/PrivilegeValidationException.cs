namespace presentation.exceptions.http;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when Privilege validation fails
/// </summary>
public class PrivilegeValidationException : BaseHttpException
{
    /// <summary>
    /// Constructor with error data
    /// </summary>
    /// <param name="errorData">Dictionary of validation errors</param>
    public PrivilegeValidationException(Dictionary<string, string[]> errorData)
        : base(400, "PRIVILEGE_VALIDATION_FAILED", "Privilege validation failed", errorData)
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Exception message</param>
    public PrivilegeValidationException(string message)
        : base(400, "PRIVILEGE_VALIDATION_FAILED", message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeValidationException(string message, Exception innerException)
        : base(400, "PRIVILEGE_VALIDATION_FAILED", message, innerException)
    {
    }
}
