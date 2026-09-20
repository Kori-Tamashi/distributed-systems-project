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
    /// Sets the City
    /// </summary>
    public AirportBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    /// <summary>
    /// Sets the Country
    /// </summary>
    public AirportBuilder WithCountry(string country)
    {
        _country = country;
        return this;
    }

    /// <summary>
    /// Builds the Airport object
    /// </summary>
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
}
