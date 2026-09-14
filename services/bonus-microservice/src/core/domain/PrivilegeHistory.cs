using core.enums;

namespace core.domain;

/// <summary>
/// Domain entity representing Privilege History record
/// </summary>
public class PrivilegeHistory
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Privilege ID this history record belongs to
    /// </summary>
    public int PrivilegeId { get; set; }

    /// <summary>
    /// Ticket UID associated with this operation
    /// </summary>
    public Guid TicketUid { get; set; }

    /// <summary>
    /// Operation datetime
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Balance change (positive for credit, negative for debit)
    /// </summary>
    public int BalanceDiff { get; set; }

    /// <summary>
    /// Operation type (FILL_IN_BALANCE, DEBIT_THE_ACCOUNT)
    /// </summary>
    public OperationType OperationType { get; set; }
}
