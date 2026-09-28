namespace presentation.exceptions.http.Flight;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when an internal server error occurs while processing Flight request
/// </summary>
public class FlightInternalServerException : BaseHttpException
{
    /// <summary>
    /// Constructor with inner exception
    /// </summary>
    /// <param name="innerException">Inner exception</param>
    public FlightInternalServerException(Exception innerException)
        : base(500, "FLIGHT_INTERNAL_ERROR", "An internal error occurred while processing Flight request", innerException)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public FlightInternalServerException(string message, Exception innerException)
        : base(500, "FLIGHT_INTERNAL_ERROR", message, innerException)
    {
    }
}
