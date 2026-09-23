using System.Net;

namespace presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Flight is not found
/// HTTP Status: 404 Not Found
/// </summary>
public class FlightNotFoundException : BaseHttpException
{
    /// <summary>
    /// Flight ID that was not found
    /// </summary>
    public int FlightId { get; }

    /// <summary>
    /// Constructor with flight ID
    /// </summary>
    /// <param name="flightId">Flight ID that was not found</param>
    public FlightNotFoundException(int flightId)
        : base((int)HttpStatusCode.NotFound, "FLIGHT_NOT_FOUND", $"Flight with ID {flightId} was not found")
    {
        FlightId = flightId;
    }

    /// <summary>
    /// Constructor with flight ID and custom message
    /// </summary>
    /// <param name="flightId">Flight ID that was not found</param>
    /// <param name="message">Custom error message</param>
    public FlightNotFoundException(int flightId, string message)
        : base((int)HttpStatusCode.NotFound, "FLIGHT_NOT_FOUND", message)
    {
        FlightId = flightId;
    }
}
