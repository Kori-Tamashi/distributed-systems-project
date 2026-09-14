using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dataaccess.models.postgres;

/// <summary>
/// PostgreSQL entity model for Ticket
/// Maps to 'tickets' table in database
/// </summary>
[Table("tickets")]
public class TicketPostgresqlModel
{
    /// <summary>
    /// Primary key - unique identifier
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
    /// Flight identifier (foreign key to Flights table)
    /// </summary>
    [Column("flight_id")]
    [Required]
    public int FlightId { get; set; }

    /// <summary>
    /// Passenger full name
    /// </summary>
    [Column("passenger_name")]
    [Required]
    [MaxLength(255)]
    public string PassengerName { get; set; } = string.Empty;

    /// <summary>
    /// Passenger email address
    /// </summary>
    [Column("passenger_email")]
    [Required]
    [MaxLength(255)]
    public string PassengerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Passenger phone number
    /// </summary>
    [Column("passenger_phone")]
    [Required]
    [MaxLength(50)]
    public string PassengerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Seat number (e.g., "12A", "23B")
    /// </summary>
    [Column("seat_number")]
    [Required]
    [MaxLength(10)]
    public string SeatNumber { get; set; } = string.Empty;

    /// <summary>
    /// Ticket class (Economy, Business, First)
    /// </summary>
    [Column("class")]
    [Required]
    public int Class { get; set; }

    /// <summary>
    /// Ticket price in rubles
    /// </summary>
    [Column("price")]
    [Required]
    public int Price { get; set; }

    /// <summary>
    /// Booking date and time
    /// </summary>
    [Column("booking_date")]
    [Required]
    public DateTime BookingDate { get; set; }

    /// <summary>
    /// Ticket status (Confirmed, Cancelled, Refunded)
    /// </summary>
    [Column("status")]
    [Required]
    public int Status { get; set; }
}
