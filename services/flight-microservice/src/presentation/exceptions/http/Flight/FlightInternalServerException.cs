using System.Net;

namespace presentation.exceptions.http;

/// <summary>
/// Exception thrown when an internal server error occurs during Flight operations
/// HTTP Status: 500 Internal Server Error
/// </summary>
public class FlightInternalServerException : BaseHttpException
{
    /// <summary>
    /// Constructor with inner exception
    /// </summary>
    /// <param name="innerException">Inner exception</param>
    public FlightInternalServerException(Exception innerException)
        : base((int)HttpStatusCode.InternalServerError, "FLIGHT_INTERNAL_ERROR", "An internal error occurred while processing Flight request", innerException)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public FlightInternalServerException(string message, Exception innerException)
        : base((int)HttpStatusCode.InternalServerError, "FLIGHT_INTERNAL_ERROR", message, innerException)
    {
    }
}
