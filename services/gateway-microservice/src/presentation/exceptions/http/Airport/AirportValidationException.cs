namespace presentation.exceptions.http.Airport;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when Airport validation fails
/// </summary>
public class AirportValidationException : BaseHttpException
{
    /// <summary>
    /// Constructor with error data
    /// </summary>
    /// <param name="errorData">Dictionary of validation errors</param>
    public AirportValidationException(Dictionary<string, string[]> errorData)
        : base(400, "AIRPORT_VALIDATION_FAILED", "Airport validation failed", errorData)
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Validation error message</param>
    public AirportValidationException(string message)
        : base(400, "AIRPORT_VALIDATION_FAILED", message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Validation error message</param>
    /// <param name="innerException">Inner exception</param>
    public AirportValidationException(string message, Exception innerException)
        : base(400, "AIRPORT_VALIDATION_FAILED", message, innerException)
    {
    }
}
