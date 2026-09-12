using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dataaccess.models.postgres;

/// <summary>
/// PostgreSQL entity model for Airport
/// Maps to 'airports' table in database
/// </summary>
[Table("airports")]
public class AirportPostgresqlModel
{
    /// <summary>
    /// Primary key - unique identifier
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Airport name
    /// </summary>
    [Column("name")]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// City where the airport is located
    /// </summary>
    [Column("city")]
    [MaxLength(255)]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Country where the airport is located
    /// </summary>
    [Column("country")]
    [MaxLength(255)]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property - flights departing from this airport
    /// </summary>
    [InverseProperty(nameof(FlightPostgresqlModel.FromAirport))]
    public virtual List<FlightPostgresqlModel> DepartingFlights { get; set; } = new();

    /// <summary>
    /// Navigation property - flights arriving to this airport
    /// </summary>
    [InverseProperty(nameof(FlightPostgresqlModel.ToAirport))]
    public virtual List<FlightPostgresqlModel> ArrivingFlights { get; set; } = new();
}
