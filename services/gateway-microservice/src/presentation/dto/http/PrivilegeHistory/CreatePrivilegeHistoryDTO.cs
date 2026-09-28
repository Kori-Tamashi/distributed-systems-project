namespace presentation.dto.http.PrivilegeHistory;

/// <summary>
/// DTO for creating PrivilegeHistory
/// </summary>
public class CreatePrivilegeHistoryDTO
{
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
    public int OperationType { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CreatePrivilegeHistoryDTO()
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public CreatePrivilegeHistoryDTO(
        int privilegeId,
        Guid ticketUid,
        DateTime dateTime,
        int balanceDiff,
        int operationType)
    {
        PrivilegeId = privilegeId;
        TicketUid = ticketUid;
        DateTime = dateTime;
        BalanceDiff = balanceDiff;
        OperationType = operationType;
    }
}
