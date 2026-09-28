using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dataaccess.models.postgres;

/// <summary>
/// PostgreSQL entity model for Privilege
/// Maps to 'privilege' table in database
/// </summary>
[Table("privilege")]
public class PrivilegePostgresqlModel
{
    /// <summary>
    /// Primary key - unique identifier
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Username (unique)
    /// </summary>
    [Column("username")]
    [Required]
    [MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Privilege status (BRONZE, SILVER, GOLD)
    /// </summary>
    [Column("status")]
    [Required]
    public int Status { get; set; }

    /// <summary>
    /// Balance in points
    /// </summary>
    [Column("balance")]
    [Required]
    public int Balance { get; set; }
}
