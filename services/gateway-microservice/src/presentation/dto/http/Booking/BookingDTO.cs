using presentation.dto.http;

namespace presentation.dto.http.Booking;

/// <summary>
/// DTO for reading Booking data (full representation)
/// </summary>
public class BookingDTO : BaseHttpDTO
{
    /// <summary>
    /// Unique booking UID (UUID)
    /// </summary>
    public Guid BookingUid { get; set; }

    /// <summary>
    /// Booking reference number (e.g., "ABC123")
    /// </summary>
    public string BookingReference { get; set; } = string.Empty;

    /// <summary>
    /// Customer full name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Customer email address
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Customer phone number
    /// </summary>
    public string CustomerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Total booking price in rubles
    /// </summary>
    public int TotalPrice { get; set; }

    /// <summary>
    /// Booking date and time
    /// </summary>
    public DateTime BookingDate { get; set; }

    /// <summary>
    /// Booking status (Confirmed, Cancelled, Refunded)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Payment method (CreditCard, PayPal, Cash)
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
        Guid bookingUid,
        string bookingReference,
        string customerName,
        string customerEmail,
        string customerPhone,
        int totalPrice,
        DateTime bookingDate,
        int status,
        int paymentMethod,
        string? paymentTransactionId = null)
        : base(id)
    {
        Id = id;
        BookingUid = bookingUid;
        BookingReference = bookingReference;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        TotalPrice = totalPrice;
        BookingDate = bookingDate;
        Status = status;
        PaymentMethod = paymentMethod;
        PaymentTransactionId = paymentTransactionId;
    }
}
