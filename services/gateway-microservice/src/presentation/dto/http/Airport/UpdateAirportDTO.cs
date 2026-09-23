namespace presentation.dto.http.Airport;

/// <summary>
/// DTO for updating Airport
/// </summary>
public class UpdateAirportDTO
{
    /// <summary>
    /// Airport name (optional)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// City where the airport is located (optional)
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Country where the airport is located (optional)
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdateAirportDTO()
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public UpdateAirportDTO(string? name = null, string? city = null, string? country = null)
    {
        Name = name;
        City = city;
        Country = country;
    }
}
