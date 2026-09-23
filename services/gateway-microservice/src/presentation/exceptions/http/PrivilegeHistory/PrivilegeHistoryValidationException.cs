namespace presentation.exceptions.http.PrivilegeHistory;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when PrivilegeHistory validation fails
/// </summary>
public class PrivilegeHistoryValidationException : BaseHttpException
{
    /// <summary>
    /// Constructor with error data
    /// </summary>
    /// <param name="errorData">Dictionary of validation errors</param>
    public PrivilegeHistoryValidationException(Dictionary<string, string[]> errorData)
        : base(400, "PRIVILEGE_HISTORY_VALIDATION_FAILED", "PrivilegeHistory validation failed", errorData)
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Validation error message</param>
    public PrivilegeHistoryValidationException(string message)
        : base(400, "PRIVILEGE_HISTORY_VALIDATION_FAILED", message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Validation error message</param>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeHistoryValidationException(string message, Exception innerException)
        : base(400, "PRIVILEGE_HISTORY_VALIDATION_FAILED", message, innerException)
    {
    }
}
