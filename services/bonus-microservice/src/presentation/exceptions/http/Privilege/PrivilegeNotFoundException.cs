namespace presentation.exceptions.http;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Privilege is not found
/// </summary>
public class PrivilegeNotFoundException : BaseHttpException
{
    /// <summary>
    /// Constructor with privilege ID
    /// </summary>
    /// <param name="privilegeId">The ID of the not found privilege</param>
    public PrivilegeNotFoundException(int privilegeId)
        : base(404, "PRIVILEGE_NOT_FOUND", $"Privilege with ID {privilegeId} not found")
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Exception message</param>
    public PrivilegeNotFoundException(string message)
        : base(404, "PRIVILEGE_NOT_FOUND", message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    public PrivilegeNotFoundException(string message, Exception innerException)
        : base(404, "PRIVILEGE_NOT_FOUND", message, innerException)
    {
    }
}
