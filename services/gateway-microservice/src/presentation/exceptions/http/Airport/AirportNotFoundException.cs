namespace presentation.exceptions.http.Airport;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when an Airport is not found
/// </summary>
public class AirportNotFoundException : BaseHttpException
{
    /// <summary>
    /// The ID of the Airport that was not found
    /// </summary>
    public int AirportId { get; }

    /// <summary>
    /// Constructor with Airport ID
    /// </summary>
    /// <param name="airportId">The ID of the Airport that was not found</param>
    public AirportNotFoundException(int airportId)
        : base(404, "AIRPORT_NOT_FOUND", $"Airport with ID {airportId} not found")
    {
        AirportId = airportId;
    }

    /// <summary>
    /// Constructor with Airport ID and inner exception
    /// </summary>
    /// <param name="airportId">The ID of the Airport that was not found</param>
    /// <param name="innerException">Inner exception</param>
    public AirportNotFoundException(int airportId, Exception innerException)
        : base(404, "AIRPORT_NOT_FOUND", $"Airport with ID {airportId} not found", innerException)
    {
        AirportId = airportId;
    }
}
