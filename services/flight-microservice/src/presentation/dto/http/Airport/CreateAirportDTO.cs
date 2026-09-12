namespace presentation.dto.http.Airport;

/// <summary>
/// DTO for creating a new Airport
/// </summary>
public class CreateAirportDTO : BaseHttpDTO
{
    /// <summary>
    /// Airport name (e.g., "Sheremetyevo International Airport")
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

    /// <summary>
    /// Default constructor
    /// </summary>
    public CreateAirportDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all required fields
    /// </summary>
    public CreateAirportDTO(
        string name,
        string city,
        string country)
        : base(0)
    {
        Name = name;
        City = city;
        Country = country;
    }
}
