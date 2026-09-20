namespace presentation.exceptions.http.PrivilegeHistory;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a PrivilegeHistory is not found
/// </summary>
public class PrivilegeHistoryNotFoundException : BaseHttpException
{
    /// <summary>
    /// The ID of the PrivilegeHistory that was not found
    /// </summary>
    public int PrivilegeHistoryId { get; }

    /// <summary>
    /// Constructor with PrivilegeHistory ID
    /// </summary>
    /// <param name="privilegeHistoryId">The ID of the PrivilegeHistory that was not found</param>
    public PrivilegeHistoryNotFoundException(int privilegeHistoryId)
        : base(404, "PRIVILEGE_HISTORY_NOT_FOUND", $"PrivilegeHistory with ID {privilegeHistoryId} not found")
    {
        PrivilegeHistoryId = privilegeHistoryId;
    }

    /// <summary>
    /// Constructor with PrivilegeHistory ID and inner exception
    /// </summary>
    /// <param name="privilegeHistoryId">The ID of the PrivilegeHistory that was not found</param>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeHistoryNotFoundException(int privilegeHistoryId, Exception innerException)
        : base(404, "PRIVILEGE_HISTORY_NOT_FOUND", $"PrivilegeHistory with ID {privilegeHistoryId} not found", innerException)
    {
        PrivilegeHistoryId = privilegeHistoryId;
    }
}
