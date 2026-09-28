namespace presentation.exceptions.http.Privilege;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Privilege is not found
/// </summary>
public class PrivilegeNotFoundException : BaseHttpException
{
    /// <summary>
    /// The ID of the Privilege that was not found
    /// </summary>
    public int PrivilegeId { get; }

    /// <summary>
    /// Constructor with Privilege ID
    /// </summary>
    /// <param name="privilegeId">The ID of the Privilege that was not found</param>
    public PrivilegeNotFoundException(int privilegeId)
        : base(404, "PRIVILEGE_NOT_FOUND", $"Privilege with ID {privilegeId} not found")
    {
        PrivilegeId = privilegeId;
    }

    /// <summary>
    /// Constructor with Privilege ID and inner exception
    /// </summary>
    /// <param name="privilegeId">The ID of the Privilege that was not found</param>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeNotFoundException(int privilegeId, Exception innerException)
        : base(404, "PRIVILEGE_NOT_FOUND", $"Privilege with ID {privilegeId} not found", innerException)
    {
        PrivilegeId = privilegeId;
    }
}
