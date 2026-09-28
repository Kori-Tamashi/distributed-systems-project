namespace presentation.exceptions.http.Privilege;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when an internal server error occurs while processing Privilege request
/// </summary>
public class PrivilegeInternalServerException : BaseHttpException
{
    /// <summary>
    /// Constructor with inner exception
    /// </summary>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeInternalServerException(Exception innerException)
        : base(500, "PRIVILEGE_INTERNAL_ERROR", "An internal error occurred while processing Privilege request", innerException)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeInternalServerException(string message, Exception innerException)
        : base(500, "PRIVILEGE_INTERNAL_ERROR", message, innerException)
    {
    }
}
