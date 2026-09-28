using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dataaccess.models.postgres;

/// <summary>
/// PostgreSQL entity model for Flight
/// Maps to 'flights' table in database
/// </summary>
[Table("flights")]
public class FlightPostgresqlModel
{
    /// <summary>
    /// Primary key - unique identifier
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Unique flight UID (UUID)
    /// </summary>
    [Column("flight_uid")]
    [Required]
    public Guid FlightUid { get; set; }

    /// <summary>
    /// Flight number (required, max 20 chars)
    /// </summary>
    [Column("flight_number")]
    [Required]
    [MaxLength(20)]
    public string FlightNumber { get; set; } = string.Empty;

    /// <summary>
    /// Flight date and time
    /// </summary>
    [Column("datetime")]
    [Required]
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Departure airport ID
    /// </summary>
    [Column("from_airport_id")]
    [Required]
    public int FromAirportId { get; set; }

    /// <summary>
    /// Arrival airport ID
    /// </summary>
    [Column("to_airport_id")]
    [Required]
    public int ToAirportId { get; set; }

    /// <summary>
    /// Flight price in rubles
    /// </summary>
    [Column("price")]
    [Required]
    public int Price { get; set; }

    /// <summary>
    /// Navigation property - departure airport
    /// </summary>
    [ForeignKey(nameof(FromAirportId))]
    public virtual AirportPostgresqlModel? FromAirport { get; set; }

    /// <summary>
    /// Navigation property - arrival airport
    /// </summary>
    [ForeignKey(nameof(ToAirportId))]
    public virtual AirportPostgresqlModel? ToAirport { get; set; }
}
