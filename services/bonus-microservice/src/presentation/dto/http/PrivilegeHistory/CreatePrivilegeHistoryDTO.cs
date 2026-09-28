using System;
using System.ComponentModel.DataAnnotations;

namespace presentation.dto.http.PrivilegeHistory;

/// <summary>
/// DTO for creating new PrivilegeHistory
/// </summary>
public class CreatePrivilegeHistoryDTO : BaseHttpDTO
{
    /// <summary>
    /// Privilege ID (required)
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int PrivilegeId { get; set; }

    /// <summary>
    /// Ticket UID associated with this operation
    /// </summary>
    public Guid TicketUid { get; set; }

    /// <summary>
    /// Operation datetime (UTC, must be in the past or present)
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Balance change (positive for credit, negative for debit, cannot be zero)
    /// </summary>
    [Required]
    public int BalanceDiffNegative { get; set; }

    /// <summary>
    /// Operation type (FILL_IN_BALANCE=0, DEBIT_THE_ACCOUNT=1)
    /// </summary>
    [Range(0, 1)]
    public int OperationType { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CreatePrivilegeHistoryDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with required fields
    /// </summary>
    public CreatePrivilegeHistoryDTO(
        int privilegeId,
        Guid ticketUid,
        DateTime dateTime,
        int balanceDiff,
        int operationType)
        : base(0)
    {
        PrivilegeId = privilegeId;
        TicketUid = ticketUid;
        DateTime = dateTime;
        BalanceDiffNegative = balanceDiff;
        OperationType = operationType;
    }
}
