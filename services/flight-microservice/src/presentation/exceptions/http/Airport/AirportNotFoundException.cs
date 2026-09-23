using System.Net;

namespace presentation.exceptions.http;

/// <summary>
/// Exception thrown when an Airport is not found
/// HTTP Status: 404 Not Found
/// </summary>
public class AirportNotFoundException : BaseHttpException
{
    /// <summary>
    /// Airport ID that was not found
    /// </summary>
    public int AirportId { get; }

    /// <summary>
    /// Constructor with airport ID
    /// </summary>
    /// <param name="airportId">Airport ID that was not found</param>
    public AirportNotFoundException(int airportId)
        : base((int)HttpStatusCode.NotFound, "AIRPORT_NOT_FOUND", $"Airport with ID {airportId} was not found")
    {
        AirportId = airportId;
    }

    /// <summary>
    /// Constructor with airport ID and custom message
    /// </summary>
    /// <param name="airportId">Airport ID that was not found</param>
    /// <param name="message">Custom error message</param>
    public AirportNotFoundException(int airportId, string message)
        : base((int)HttpStatusCode.NotFound, "AIRPORT_NOT_FOUND", message)
    {
        AirportId = airportId;
    }
}
