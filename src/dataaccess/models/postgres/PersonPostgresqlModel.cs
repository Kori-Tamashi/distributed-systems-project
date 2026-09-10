using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dataaccess.models.postgres;

/// <summary>
/// PostgreSQL entity model for Person
/// Maps to 'persons' table in database
/// </summary>
[Table("persons")]
public class PersonPostgresqlModel
{
    /// <summary>
    /// Primary key - unique identifier
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Person's full name (required, not null)
    /// </summary>
    [Column("name")]
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Person's age (optional)
    /// </summary>
    [Column("age")]
    public int? Age { get; set; }

    /// <summary>
    /// Person's address (optional)
    /// </summary>
    [Column("address")]
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// Person's workplace or job (optional)
    /// </summary>
    [Column("work")]
    [MaxLength(255)]
    public string? Work { get; set; }
}
