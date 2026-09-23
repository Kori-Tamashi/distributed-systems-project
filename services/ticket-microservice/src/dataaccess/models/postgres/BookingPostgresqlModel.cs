using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dataaccess.models.postgres;

/// <summary>
/// PostgreSQL entity model for Booking
/// Maps to 'bookings' table in database
/// </summary>
[Table("bookings")]
public class BookingPostgresqlModel
{
    /// <summary>
    /// Primary key - unique identifier
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Unique booking UID (UUID)
    /// </summary>
    [Column("booking_uid")]
    [Required]
    public Guid BookingUid { get; set; }

    /// <summary>
    /// Booking reference number (e.g., "ABC123")
    /// </summary>
    [Column("booking_reference")]
    [Required]
    [MaxLength(50)]
    public string BookingReference { get; set; } = string.Empty;

    /// <summary>
    /// Customer full name
    /// </summary>
    [Column("customer_name")]
    [Required]
    [MaxLength(255)]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Customer email address
    /// </summary>
    [Column("customer_email")]
    [Required]
    [MaxLength(255)]
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Customer phone number
    /// </summary>
    [Column("customer_phone")]
    [Required]
    [MaxLength(50)]
    public string CustomerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Total booking price in rubles
    /// </summary>
    [Column("total_price")]
    [Required]
    public int TotalPrice { get; set; }

    /// <summary>
    /// Booking date and time
    /// </summary>
    [Column("booking_date")]
    [Required]
    public DateTime BookingDate { get; set; }

    /// <summary>
    /// Booking status (Confirmed, Cancelled, Refunded)
    /// </summary>
    [Column("status")]
    [Required]
    public int Status { get; set; }

    /// <summary>
    /// Payment method (CreditCard, PayPal, Cash)
    /// </summary>
    [Column("payment_method")]
    [Required]
    public int PaymentMethod { get; set; }

    /// <summary>
    /// Payment transaction ID (optional)
    /// </summary>
    [Column("payment_transaction_id")]
    [MaxLength(255)]
    public string? PaymentTransactionId { get; set; }

    /// <summary>
}
