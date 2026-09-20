namespace presentation.dto.http.Booking;

/// <summary>
/// DTO for updating Booking
/// </summary>
public class UpdateBookingDTO
{
    /// <summary>
    /// Booking reference number (e.g., "ABC123") (optional)
    /// </summary>
    public string? BookingReference { get; set; }

    /// <summary>
    /// Customer full name (optional)
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Customer email address (optional)
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Customer phone number (optional)
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Total booking price in rubles (optional)
    /// </summary>
    public int? TotalPrice { get; set; }

    /// <summary>
    /// Booking date and time (optional)
    /// </summary>
    public DateTime? BookingDate { get; set; }

    /// <summary>
    /// Booking status (Confirmed, Cancelled, Refunded) (optional)
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Payment method (CreditCard, PayPal, Cash) (optional)
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
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public UpdateBookingDTO(
        string? bookingReference = null,
        string? customerName = null,
        string? customerEmail = null,
        string? customerPhone = null,
        int? totalPrice = null,
        DateTime? bookingDate = null,
        int? status = null,
        int? paymentMethod = null,
        string? paymentTransactionId = null)
    {
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
