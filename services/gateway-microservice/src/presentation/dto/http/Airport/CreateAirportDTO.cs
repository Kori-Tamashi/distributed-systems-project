namespace presentation.dto.http.Airport;

/// <summary>
/// DTO for creating a new Airport
/// </summary>
public class CreateAirportDTO
{
    /// <summary>
    /// Airport name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// City where the airport is located
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Country where the airport is located
    /// </summary>
    public string Country { get; set; } = string.Empty;
}
