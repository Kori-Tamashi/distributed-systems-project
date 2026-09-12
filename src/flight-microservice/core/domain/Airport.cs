namespace core.domain;

/// <summary>
/// Domain entity representing an Airport
/// </summary>
public class Airport
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

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

    /// <summary>
    /// Navigation property - flights departing from this airport
    /// </summary>
    public List<Flight> DepartingFlights { get; set; } = new();

    /// <summary>
    /// Navigation property - flights arriving to this airport
    /// </summary>
    public List<Flight> ArrivingFlights { get; set; } = new();
}
