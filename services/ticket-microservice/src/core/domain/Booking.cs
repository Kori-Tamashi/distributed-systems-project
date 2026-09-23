using core.enums;

using System;

namespace core.domain;

/// <summary>
/// Domain entity representing a Booking
/// </summary>
public class Booking
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

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
    public BookingStatus Status { get; set; }

    /// <summary>
    /// Payment method (CreditCard, PayPal, Cash)
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Payment transaction ID (optional)
    /// </summary>
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// Navigation property - tickets in this booking
    /// </summary>
    public List<Ticket> Tickets { get; set; } = new();

    /// <summary>
    /// Timestamp when the booking was created (UTC)
    /// </summary>

    /// <summary>
    /// Timestamp when the booking was last updated (UTC)
    /// </summary>
}
