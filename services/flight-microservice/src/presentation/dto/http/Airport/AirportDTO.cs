namespace presentation.dto.http.Airport;

/// <summary>
/// DTO for reading Airport data (full representation)
/// </summary>
public class AirportDTO : BaseHttpDTO
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
    public AirportDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public AirportDTO(
        int id,
        string name,
        string city,
        string country,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
        : base(id)
    {
        Id = id;
        Name = name;
        City = city;
        Country = country;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
