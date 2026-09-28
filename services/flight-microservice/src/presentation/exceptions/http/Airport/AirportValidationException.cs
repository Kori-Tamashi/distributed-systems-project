using System.Net;

namespace presentation.exceptions.http;

/// <summary>
/// Exception thrown when Airport validation fails
/// HTTP Status: 400 Bad Request
/// </summary>
public class AirportValidationException : BaseHttpException
{
    /// <summary>
    /// Validation errors by field name
    /// </summary>
    public Dictionary<string, string[]> ValidationErrors { get; }

    /// <summary>
    /// Constructor with errors dictionary
    /// </summary>
    /// <param name="errors">Dictionary of validation errors by field name</param>
    public AirportValidationException(Dictionary<string, string[]> errors)
        : base((int)HttpStatusCode.BadRequest, "AIRPORT_VALIDATION_FAILED", $"Airport validation failed with {errors.Count} error(s)")
    {
        ValidationErrors = errors;
        ErrorData = errors;
    }

    /// <summary>
    /// Constructor with single error
    /// </summary>
    /// <param name="fieldName">Field name with error</param>
    /// <param name="errorMessage">Error message</param>
    public AirportValidationException(string fieldName, string errorMessage)
        : base((int)HttpStatusCode.BadRequest, "AIRPORT_VALIDATION_FAILED", "Airport validation failed")
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
    public AirportValidationException(string message)
        : base((int)HttpStatusCode.BadRequest, "AIRPORT_VALIDATION_FAILED", message)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }
}
