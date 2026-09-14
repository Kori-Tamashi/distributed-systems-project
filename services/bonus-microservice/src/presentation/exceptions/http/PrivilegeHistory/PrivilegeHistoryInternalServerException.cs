namespace presentation.exceptions.http;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when an internal server error occurs during PrivilegeHistory operations
/// </summary>
public class PrivilegeHistoryInternalServerException : BaseHttpException
{
    /// <summary>
    /// Constructor with inner exception
    /// </summary>
    /// <param name="innerException">The underlying exception</param>
    public PrivilegeHistoryInternalServerException(Exception innerException)
        : base(500, "PRIVILEGE_HISTORY_INTERNAL_ERROR", "An internal error occurred while processing Privilege history request", innerException)
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Exception message</param>
    public PrivilegeHistoryInternalServerException(string message)
        : base(500, "PRIVILEGE_HISTORY_INTERNAL_ERROR", message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeHistoryInternalServerException(string message, Exception innerException)
        : base(500, "PRIVILEGE_HISTORY_INTERNAL_ERROR", message, innerException)
    {
    }
}
