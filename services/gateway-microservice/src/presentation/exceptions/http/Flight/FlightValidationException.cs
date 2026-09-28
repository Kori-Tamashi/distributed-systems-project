namespace presentation.exceptions.http.Flight;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when Flight validation fails
/// </summary>
public class FlightValidationException : BaseHttpException
{
    /// <summary>
    /// Constructor with error data
    /// </summary>
    /// <param name="errorData">Dictionary of validation errors</param>
    public FlightValidationException(Dictionary<string, string[]> errorData)
        : base(400, "FLIGHT_VALIDATION_FAILED", "Flight validation failed", errorData)
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Validation error message</param>
    public FlightValidationException(string message)
        : base(400, "FLIGHT_VALIDATION_FAILED", message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Validation error message</param>
    /// <param name="innerException">Inner exception</param>
    public FlightValidationException(string message, Exception innerException)
        : base(400, "FLIGHT_VALIDATION_FAILED", message, innerException)
    {
    }
}
