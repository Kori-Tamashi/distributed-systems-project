using System;

namespace dataaccess.dto.http.Booking;

/// <summary>
/// DTO for reading Booking data (full representation)
/// </summary>
public class BookingDTO : BaseHttpDTO
{
    /// <summary>
    /// Booking reference number (e.g., "BK100", "BK200")
    /// </summary>
    public string BookingReference { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the booking (GUID)
    /// </summary>
    public Guid BookingUid { get; set; }

    /// <summary>
    /// Customer name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Customer email
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Customer phone
    /// </summary>
    public string CustomerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Booking date and time (UTC)
    /// </summary>
    public DateTime BookingDate { get; set; }

    /// <summary>
    /// Total price in rubles
    /// </summary>
    public int TotalPrice { get; set; }

    /// <summary>
    /// Booking status
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Payment method
    /// </summary>
    public int PaymentMethod { get; set; }

    /// <summary>
    /// Payment transaction ID (optional)
    /// </summary>
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public BookingDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public BookingDTO(
        int id,
        string bookingReference,
        Guid bookingUid,
        string customerName,
        string customerEmail,
        string customerPhone,
        DateTime bookingDate,
        int totalPrice,
        int status,
        int paymentMethod,
        string? paymentTransactionId = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
        : base(id)
    {
        Id = id;
        BookingReference = bookingReference;
        BookingUid = bookingUid;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        BookingDate = bookingDate;
        TotalPrice = totalPrice;
        Status = status;
        PaymentMethod = paymentMethod;
        PaymentTransactionId = paymentTransactionId;
    }
}
