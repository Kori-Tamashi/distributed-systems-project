namespace presentation.exceptions.http.PrivilegeHistory;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when an internal server error occurs while processing PrivilegeHistory request
/// </summary>
public class PrivilegeHistoryInternalServerException : BaseHttpException
{
    /// <summary>
    /// Constructor with inner exception
    /// </summary>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeHistoryInternalServerException(Exception innerException)
        : base(500, "PRIVILEGE_HISTORY_INTERNAL_ERROR", "An internal error occurred while processing PrivilegeHistory request", innerException)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeHistoryInternalServerException(string message, Exception innerException)
        : base(500, "PRIVILEGE_HISTORY_INTERNAL_ERROR", message, innerException)
    {
    }
}
