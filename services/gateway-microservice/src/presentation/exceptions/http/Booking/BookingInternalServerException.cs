namespace presentation.exceptions.http.Booking;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when an internal server error occurs while processing Booking request
/// </summary>
public class BookingInternalServerException : BaseHttpException
{
    /// <summary>
    /// Constructor with inner exception
    /// </summary>
    /// <param name="innerException">Inner exception</param>
    public BookingInternalServerException(Exception innerException)
        : base(500, "BOOKING_INTERNAL_ERROR", "An internal error occurred while processing Booking request", innerException)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public BookingInternalServerException(string message, Exception innerException)
        : base(500, "BOOKING_INTERNAL_ERROR", message, innerException)
    {
    }
}
