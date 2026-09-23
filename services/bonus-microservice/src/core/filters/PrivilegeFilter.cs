using core.enums;

namespace core.filters;

/// <summary>
/// Filter criteria for querying Privileges from repository
/// Used for filtering and searching privileges in data access layer
/// </summary>
public class PrivilegeFilter
{
    /// <summary>
    /// Filter by username (partial match, case-insensitive)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Filter by privilege status
    /// </summary>
    public PrivilegeStatus? Status { get; set; }

    /// <summary>
    /// Filter by minimum balance
    /// </summary>
    public int? MinBalance { get; set; }

    /// <summary>
    /// Filter by maximum balance
    /// </summary>
    public int? MaxBalance { get; set; }
}
