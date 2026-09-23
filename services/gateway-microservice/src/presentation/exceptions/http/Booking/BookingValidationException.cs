namespace presentation.exceptions.http.Booking;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when Booking validation fails
/// </summary>
public class BookingValidationException : BaseHttpException
{
    /// <summary>
    /// Constructor with error data
    /// </summary>
    /// <param name="errorData">Dictionary of validation errors</param>
    public BookingValidationException(Dictionary<string, string[]> errorData)
        : base(400, "BOOKING_VALIDATION_FAILED", "Booking validation failed", errorData)
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Validation error message</param>
    public BookingValidationException(string message)
        : base(400, "BOOKING_VALIDATION_FAILED", message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Validation error message</param>
    /// <param name="innerException">Inner exception</param>
    public BookingValidationException(string message, Exception innerException)
        : base(400, "BOOKING_VALIDATION_FAILED", message, innerException)
    {
    }
}
