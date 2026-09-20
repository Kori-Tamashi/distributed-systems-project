namespace presentation.exceptions.http.Flight;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a Flight is not found
/// </summary>
public class FlightNotFoundException : BaseHttpException
{
    /// <summary>
    /// The ID of the Flight that was not found
    /// </summary>
    public int FlightId { get; }

    /// <summary>
    /// Constructor with Flight ID
    /// </summary>
    /// <param name="flightId">The ID of the Flight that was not found</param>
    public FlightNotFoundException(int flightId)
        : base(404, "FLIGHT_NOT_FOUND", $"Flight with ID {flightId} not found")
    {
        FlightId = flightId;
    }

    /// <summary>
    /// Constructor with Flight ID and inner exception
    /// </summary>
    /// <param name="flightId">The ID of the Flight that was not found</param>
    /// <param name="innerException">Inner exception</param>
    public FlightNotFoundException(int flightId, Exception innerException)
        : base(404, "FLIGHT_NOT_FOUND", $"Flight with ID {flightId} not found", innerException)
    {
        FlightId = flightId;
    }
}
