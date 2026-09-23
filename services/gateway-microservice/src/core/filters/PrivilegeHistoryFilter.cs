using core.enums;

namespace core.filters;

/// <summary>
/// Filter criteria for querying PrivilegeHistory from repository
/// Used for filtering and searching history records in data access layer
/// </summary>
public class PrivilegeHistoryFilter
{
    /// <summary>
    /// Filter by privilege ID
    /// </summary>
    public int? PrivilegeId { get; set; }

    /// <summary>
    /// Filter by ticket UID
    /// </summary>
    public Guid? TicketUid { get; set; }

    /// <summary>
    /// Filter by operation type
    /// </summary>
    public OperationType? OperationType { get; set; }

    /// <summary>
    /// Filter by minimum balance diff
    /// </summary>
    public int? MinBalanceDiff { get; set; }

    /// <summary>
    /// Filter by maximum balance diff
    /// </summary>
    public int? MaxBalanceDiff { get; set; }

    /// <summary>
    /// Filter by datetime (from this date)
    /// </summary>
    public DateTime? DateTimeFrom { get; set; }

    /// <summary>
    /// Filter by datetime (until this date)
    /// </summary>
    public DateTime? DateTimeUntil { get; set; }
}
