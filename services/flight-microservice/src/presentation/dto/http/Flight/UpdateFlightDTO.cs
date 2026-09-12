using System;

namespace presentation.dto.http.Flight;

/// <summary>
/// DTO for updating an existing Flight
/// All properties are nullable for partial updates
/// </summary>
public class UpdateFlightDTO : BaseHttpDTO
{
    /// <summary>
    /// Flight number (e.g., "SU100", "AF200")
    /// </summary>
    public string? FlightNumber { get; set; }

    /// <summary>
    /// Unique identifier for the flight (GUID)
    /// </summary>
    public Guid? FlightUid { get; set; }

    /// <summary>
    /// Scheduled departure date and time (UTC)
    /// </summary>
    public DateTime? DateTime { get; set; }

    /// <summary>
    /// Departure airport ID
    /// </summary>
    public int? FromAirportId { get; set; }

    /// <summary>
    /// Arrival airport ID
    /// </summary>
    public int? ToAirportId { get; set; }

    /// <summary>
    /// Flight price in rubles
    /// </summary>
    public int? Price { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdateFlightDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with ID
    /// </summary>
    /// <param name="id">Flight identifier</param>
    public UpdateFlightDTO(int id)
        : base(id)
    {
    }
}
