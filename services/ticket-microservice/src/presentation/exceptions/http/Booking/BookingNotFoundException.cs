using System.Net;

namespace presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Booking is not found
/// HTTP Status: 404 Not Found
/// </summary>
public class BookingNotFoundException : BaseHttpException
{
    /// <summary>
    /// Booking ID that was not found
    /// </summary>
    public int BookingId { get; }

    /// <summary>
    /// Constructor with booking ID
    /// </summary>
    /// <param name="bookingId">Booking ID that was not found</param>
    public BookingNotFoundException(int bookingId)
        : base((int)HttpStatusCode.NotFound, "BOOKING_NOT_FOUND", $"Booking with ID {bookingId} was not found")
    {
        BookingId = bookingId;
    }

    /// <summary>
    /// Constructor with booking ID and custom message
    /// </summary>
    /// <param name="bookingId">Booking ID that was not found</param>
    /// <param name="message">Custom error message</param>
    public BookingNotFoundException(int bookingId, string message)
        : base((int)HttpStatusCode.NotFound, "BOOKING_NOT_FOUND", message)
    {
        BookingId = bookingId;
    }
}
