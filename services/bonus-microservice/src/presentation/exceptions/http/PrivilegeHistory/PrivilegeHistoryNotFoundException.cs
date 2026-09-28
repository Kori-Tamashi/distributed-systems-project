namespace presentation.exceptions.http;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a PrivilegeHistory is not found
/// </summary>
public class PrivilegeHistoryNotFoundException : BaseHttpException
{
    /// <summary>
    /// Constructor with privilege history ID
    /// </summary>
    /// <param name="privilegeHistoryId">The ID of the not found privilege history</param>
    public PrivilegeHistoryNotFoundException(int privilegeHistoryId)
        : base(404, "PRIVILEGE_HISTORY_NOT_FOUND", $"Privilege history with ID {privilegeHistoryId} not found")
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Exception message</param>
    public PrivilegeHistoryNotFoundException(string message)
        : base(404, "PRIVILEGE_HISTORY_NOT_FOUND", message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeHistoryNotFoundException(string message, Exception innerException)
        : base(404, "PRIVILEGE_HISTORY_NOT_FOUND", message, innerException)
    {
    }
}
