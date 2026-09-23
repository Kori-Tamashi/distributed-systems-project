namespace presentation.exceptions.http.Booking;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Booking business rule is violated
/// </summary>
public class BookingBusinessRuleViolationException : BaseHttpException
{
    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Business rule violation message</param>
    public BookingBusinessRuleViolationException(string message)
        : base(409, "BOOKING_BUSINESS_RULE_VIOLATION", message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Business rule violation message</param>
    /// <param name="innerException">Inner exception</param>
    public BookingBusinessRuleViolationException(string message, Exception innerException)
        : base(409, "BOOKING_BUSINESS_RULE_VIOLATION", message, innerException)
    {
    }
}
