using System;

namespace dataaccess.dto.http.PrivilegeHistory;

/// <summary>
/// DTO for reading PrivilegeHistory data (full representation)
/// </summary>
public class PrivilegeHistoryDTO : BaseHttpDTO
{
    /// <summary>
    /// Privilege ID
    /// </summary>
    public int PrivilegeId { get; set; }

    /// <summary>
    /// Ticket UID associated with this operation
    /// </summary>
    public Guid TicketUid { get; set; }

    /// <summary>
    /// Operation datetime (UTC)
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Balance change (positive for credit, negative for debit)
    /// </summary>
    public int BalanceDiff { get; set; }

    /// <summary>
    /// Operation type (FILL_IN_BALANCE=0, DEBIT_THE_ACCOUNT=1)
    /// </summary>
    public int OperationType { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public PrivilegeHistoryDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public PrivilegeHistoryDTO(
        int id,
        int privilegeId,
        Guid ticketUid,
        DateTime dateTime,
        int balanceDiff,
        int operationType)
        : base(id)
    {
        Id = id;
        PrivilegeId = privilegeId;
        TicketUid = ticketUid;
        DateTime = dateTime;
        BalanceDiff = balanceDiff;
        OperationType = operationType;
    }
}
