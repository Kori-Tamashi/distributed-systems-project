namespace presentation.dto.http.Airport;

/// <summary>
/// DTO for updating an existing Airport
/// All properties are nullable for partial updates
/// </summary>
public class UpdateAirportDTO : BaseHttpDTO
{
    /// <summary>
    /// Airport name (e.g., "Sheremetyevo International Airport")
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// City where the airport is located
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Country where the airport is located
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdateAirportDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with ID
    /// </summary>
    /// <param name="id">Airport identifier</param>
    public UpdateAirportDTO(int id)
        : base(id)
    {
    }
}
