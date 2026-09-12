using core.domain;

namespace tests.fixtures.builders;

/// <summary>
/// Test Data Builder for Airport entity
/// Allows fluent construction of Airport objects for testing
/// </summary>
public class AirportBuilder
{
    private int _id = 1;
    private string _name = "Sheremetyevo International Airport";
    private string _city = "Moscow";
    private string _country = "Russia";

    /// <summary>
    /// Sets the Airport Id
    /// </summary>
    public AirportBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the Airport Name
    /// </summary>
    public AirportBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the Airport City
    /// </summary>
    public AirportBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    /// <summary>
    /// Sets the Airport Country
    /// </summary>
    public AirportBuilder WithCountry(string country)
    {
        _country = country;
        return this;
    }

    /// <summary>
    /// Builds the Airport object with current configuration
    /// </summary>
    /// <returns>Airport entity with configured properties</returns>
    public Airport Build()
    {
        return new Airport
        {
            Id = _id,
            Name = _name,
            City = _city,
            Country = _country
        };
    }

    /// <summary>
    /// Builds a list of Airport objects
    /// </summary>
    /// <param name="count">Number of airports to build</param>
    /// <param name="incrementIds">Whether to increment Ids for each airport</param>
    /// <returns>List of Airport entities</returns>
    public List<Airport> BuildList(int count, bool incrementIds = true)
    {
        var airports = new List<Airport>();
        
        for (int i = 0; i < count; i++)
        {
            var airport = new Airport
            {
                Id = incrementIds ? _id + i : _id,
                Name = $"{_name} {i}",
                City = _city,
                Country = _country
            };
            airports.Add(airport);
        }

        return airports;
    }

    /// <summary>
    /// Creates an Airport with random Id
    /// </summary>
    public Airport BuildWithRandomId()
    {
        return new Airport
        {
            Id = new Random().Next(1, int.MaxValue),
            Name = _name,
            City = _city,
            Country = _country
        };
    }
}
