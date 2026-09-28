using System;
using dataaccess.dto.http;
using System.ComponentModel.DataAnnotations;

namespace dataaccess.dto.http.PrivilegeHistory;

/// <summary>
/// DTO for updating existing PrivilegeHistory
/// </summary>
public class UpdatePrivilegeHistoryDTO : BaseHttpDTO
{
    /// <summary>
    /// Privilege ID
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? PrivilegeId { get; set; }

    /// <summary>
    /// Ticket UID
    /// </summary>
    public Guid? TicketUid { get; set; }

    /// <summary>
    /// Operation datetime
    /// </summary>
    public DateTime? DateTime { get; set; }

    /// <summary>
    /// Balance change (cannot be zero)
    /// </summary>
    public int? BalanceDiffNegative { get; set; }

    /// <summary>
    /// Operation type
    /// </summary>
    [Range(0, 1)]
    public int? OperationType { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdatePrivilegeHistoryDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with ID and optional fields
    /// </summary>
    public UpdatePrivilegeHistoryDTO(
        int id,
        int? privilegeId = null,
        Guid? ticketUid = null,
        DateTime? dateTime = null,
        int? balanceDiff = null,
        int? operationType = null)
        : base(id)
    {
        Id = id;
        PrivilegeId = privilegeId;
        TicketUid = ticketUid;
        DateTime = dateTime;
        BalanceDiffNegative = balanceDiff;
        OperationType = operationType;
    }
}
