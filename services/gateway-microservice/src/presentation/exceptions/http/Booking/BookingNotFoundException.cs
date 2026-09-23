namespace presentation.exceptions.http.Booking;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Booking is not found
/// </summary>
public class BookingNotFoundException : BaseHttpException
{
    /// <summary>
    /// The ID of the Booking that was not found
    /// </summary>
    public int BookingId { get; }

    /// <summary>
    /// Constructor with Booking ID
    /// </summary>
    /// <param name="bookingId">The ID of the Booking that was not found</param>
    public BookingNotFoundException(int bookingId)
        : base(404, "BOOKING_NOT_FOUND", $"Booking with ID {bookingId} not found")
    {
        BookingId = bookingId;
    }

    /// <summary>
    /// Constructor with Booking ID and inner exception
    /// </summary>
    /// <param name="bookingId">The ID of the Booking that was not found</param>
    /// <param name="innerException">Inner exception</param>
    public BookingNotFoundException(int bookingId, Exception innerException)
        : base(404, "BOOKING_NOT_FOUND", $"Booking with ID {bookingId} not found", innerException)
    {
        BookingId = bookingId;
    }
}
