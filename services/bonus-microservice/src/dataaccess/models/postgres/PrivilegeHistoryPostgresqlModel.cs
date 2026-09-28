using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dataaccess.models.postgres;

/// <summary>
/// PostgreSQL entity model for PrivilegeHistory
/// Maps to 'privilege_history' table in database
/// </summary>
[Table("privilege_history")]
public class PrivilegeHistoryPostgresqlModel
{
    /// <summary>
    /// Primary key - unique identifier
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Privilege ID this history record belongs to (foreign key)
    /// </summary>
    [Column("privilege_id")]
    [Required]
    public int PrivilegeId { get; set; }

    /// <summary>
    /// Ticket UID associated with this operation
    /// </summary>
    [Column("ticket_uid")]
    [Required]
    public Guid TicketUid { get; set; }

    /// <summary>
    /// Operation datetime
    /// </summary>
    [Column("datetime")]
    [Required]
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Balance change (positive for credit, negative for debit)
    /// </summary>
    [Column("balance_diff")]
    [Required]
    public int BalanceDiff { get; set; }

    /// <summary>
    /// Operation type (FILL_IN_BALANCE, DEBIT_THE_ACCOUNT)
    /// </summary>
    [Column("operation_type")]
    [Required]
    public int OperationType { get; set; }
}
