namespace presentation.exceptions.http.Airport;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when an internal server error occurs while processing Airport request
/// </summary>
public class AirportInternalServerException : BaseHttpException
{
    /// <summary>
    /// Constructor with inner exception
    /// </summary>
    /// <param name="innerException">Inner exception</param>
    public AirportInternalServerException(Exception innerException)
        : base(500, "AIRPORT_INTERNAL_ERROR", "An internal error occurred while processing Airport request", innerException)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public AirportInternalServerException(string message, Exception innerException)
        : base(500, "AIRPORT_INTERNAL_ERROR", message, innerException)
    {
    }
}
