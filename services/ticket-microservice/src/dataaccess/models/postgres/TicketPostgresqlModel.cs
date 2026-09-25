using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using core.enums;

namespace dataaccess.models.postgres;

/// <summary>
/// PostgreSQL entity model for Ticket (per lab2-template v1 spec)
/// Maps to 'ticket' table with columns: id, ticket_uid, username, flight_number, price, status
/// </summary>
[Table("ticket")]
public class TicketPostgresqlModel
{
    /// <summary>
    /// Primary key - auto increment
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Unique ticket UID (UUID)
    /// </summary>
    [Column("ticket_uid")]
    [Required]
    public Guid TicketUid { get; set; }

    /// <summary>
    /// Username of the ticket owner
    /// </summary>
    [Column("username")]
    [Required]
    [MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Flight number (e.g., "AFL031")
    /// </summary>
    [Column("flight_number")]
    [Required]
    [MaxLength(20)]
    public string FlightNumber { get; set; } = string.Empty;

    /// <summary>
    /// Ticket price in rubles
    /// </summary>
    [Column("price")]
    [Required]
    public int Price { get; set; }

    /// <summary>
    /// Ticket status (PAID, CANCELED)
    /// </summary>
    [Column("status")]
    [Required]
    public TicketStatus Status { get; set; }
}
