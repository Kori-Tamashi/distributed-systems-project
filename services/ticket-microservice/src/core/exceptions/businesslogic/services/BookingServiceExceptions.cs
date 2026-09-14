using core.exceptions;

namespace core.exceptions.businesslogic.services;

/// <summary>
/// Exception thrown when Booking is not found in service operations
/// </summary>
public class BookingNotFoundException : ServiceEntityNotFoundException
{
    public int BookingId { get; }

    public BookingNotFoundException(int bookingId) 
        : base("Booking", bookingId)
    {
        BookingId = bookingId;
    }

    public BookingNotFoundException(int bookingId, Exception innerException) 
        : base("Booking", bookingId, innerException)
    {
        BookingId = bookingId;
    }
}

/// <summary>
/// Exception thrown when Booking validation fails
/// </summary>
public class BookingValidationException : ValidationException
{
    public BookingValidationException() : base("Booking validation failed") { }

    public BookingValidationException(string message) : base(message) { }

    public BookingValidationException(string message, Dictionary<string, string[]> errors) 
        : base(message, errors) { }

    public BookingValidationException(Exception innerException) 
        : base("Booking validation failed", innerException) { }
}

/// <summary>
/// Exception thrown when Booking business rule is violated
/// </summary>
public class BookingBusinessRuleViolationException : BusinessRuleViolationException
{
    public BookingBusinessRuleViolationException(string ruleName) : base(ruleName) { }

    public BookingBusinessRuleViolationException(string ruleName, string message) 
        : base(ruleName, message) { }

    public BookingBusinessRuleViolationException(string ruleName, Exception innerException) 
        : base(ruleName, innerException) { }
}
