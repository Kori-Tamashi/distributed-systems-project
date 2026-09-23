using System.Net;

namespace presentation.exceptions.http;

/// <summary>
/// Exception thrown when Booking validation fails
/// HTTP Status: 400 Bad Request
/// </summary>
public class BookingValidationException : BaseHttpException
{
    /// <summary>
    /// Validation errors by field name
    /// </summary>
    public Dictionary<string, string[]> ValidationErrors { get; }

    /// <summary>
    /// Constructor with errors dictionary
    /// </summary>
    /// <param name="errors">Dictionary of validation errors by field name</param>
    public BookingValidationException(Dictionary<string, string[]> errors)
        : base((int)HttpStatusCode.BadRequest, "BOOKING_VALIDATION_FAILED", $"Booking validation failed with {errors.Count} error(s)")
    {
        ValidationErrors = errors;
        ErrorData = errors;
    }

    /// <summary>
    /// Constructor with single error
    /// </summary>
    /// <param name="fieldName">Field name with error</param>
    /// <param name="errorMessage">Error message</param>
    public BookingValidationException(string fieldName, string errorMessage)
        : base((int)HttpStatusCode.BadRequest, "BOOKING_VALIDATION_FAILED", "Booking validation failed")
    {
        ValidationErrors = new Dictionary<string, string[]>
        {
            [fieldName] = new[] { errorMessage }
        };
        ErrorData = ValidationErrors;
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Error message</param>
    public BookingValidationException(string message)
        : base((int)HttpStatusCode.BadRequest, "BOOKING_VALIDATION_FAILED", message)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }
}
