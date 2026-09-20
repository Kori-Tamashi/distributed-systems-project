using core.domain;

using core.enums;

namespace core.filters;

/// <summary>
/// Filter criteria for querying Bookings from repository
/// Used for filtering and searching bookings in data access layer
/// </summary>
public class BookingFilter
{
    /// <summary>
    /// Filter by booking reference (partial match, case-insensitive)
    /// </summary>
    public string? BookingReference { get; set; }

    /// <summary>
    /// Filter by customer name (partial match, case-insensitive)
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Filter by customer email (partial match, case-insensitive)
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Filter by minimum total price
    /// </summary>
    public int? MinTotalPrice { get; set; }

    /// <summary>
    /// Filter by maximum total price
    /// </summary>
    public int? MaxTotalPrice { get; set; }

    /// <summary>
    /// Filter by minimum booking date
    /// </summary>
    public DateTime? MinBookingDate { get; set; }

    /// <summary>
    /// Filter by maximum booking date
    /// </summary>
    public DateTime? MaxBookingDate { get; set; }

    /// <summary>
    /// Filter by booking status
    /// </summary>
    public BookingStatus? Status { get; set; }

    /// <summary>
    /// Filter by payment method
    /// </summary>
    public PaymentMethod? PaymentMethod { get; set; }
}
