using System;
using dataaccess.dto.http;

namespace dataaccess.dto.http.Booking;

/// <summary>
/// DTO for updating an existing Booking
/// All properties are nullable for partial updates
/// </summary>
public class UpdateBookingDTO : BaseHttpDTO
{
    /// <summary>
    /// Booking reference number (e.g., "BK100", "BK200")
    /// </summary>
    public string? BookingReference { get; set; }

    /// <summary>
    /// Unique identifier for the booking (GUID)
    /// </summary>
    public Guid? BookingUid { get; set; }

    /// <summary>
    /// Customer name
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Customer email
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Customer phone
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Booking date and time (UTC)
    /// </summary>
    public DateTime? BookingDate { get; set; }

    /// <summary>
    /// Total price in rubles
    /// </summary>
    public int? TotalPrice { get; set; }

    /// <summary>
    /// Booking status
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Payment method
    /// </summary>
    public int? PaymentMethod { get; set; }

    /// <summary>
    /// Payment transaction ID (optional)
    /// </summary>
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdateBookingDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with ID
    /// </summary>
    /// <param name="id">Booking identifier</param>
    public UpdateBookingDTO(int id)
        : base(id)
    {
    }
}
