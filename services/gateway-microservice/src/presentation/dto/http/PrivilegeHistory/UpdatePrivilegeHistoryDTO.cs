namespace presentation.dto.http.PrivilegeHistory;

/// <summary>
/// DTO for updating PrivilegeHistory
/// </summary>
public class UpdatePrivilegeHistoryDTO
{
    /// <summary>
    /// Privilege ID this history record belongs to (optional)
    /// </summary>
    public int? PrivilegeId { get; set; }

    /// <summary>
    /// Ticket UID associated with this operation (optional)
    /// </summary>
    public Guid? TicketUid { get; set; }

    /// <summary>
    /// Operation datetime (optional)
    /// </summary>
    public DateTime? DateTime { get; set; }

    /// <summary>
    /// Balance change (positive for credit, negative for debit) (optional)
    /// </summary>
    public int? BalanceDiff { get; set; }

    /// <summary>
    /// Operation type (FILL_IN_BALANCE, DEBIT_THE_ACCOUNT) (optional)
    /// </summary>
    public int? OperationType { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdatePrivilegeHistoryDTO()
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public UpdatePrivilegeHistoryDTO(
        int? privilegeId = null,
        Guid? ticketUid = null,
        DateTime? dateTime = null,
        int? balanceDiff = null,
        int? operationType = null)
    {
        PrivilegeId = privilegeId;
        TicketUid = ticketUid;
        DateTime = dateTime;
        BalanceDiff = balanceDiff;
        OperationType = operationType;
    }
}
