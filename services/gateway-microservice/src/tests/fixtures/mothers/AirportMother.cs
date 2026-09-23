using core.domain;
using tests.fixtures.builders;

namespace tests.fixtures.mothers;

/// <summary>
/// Object Mother for Airport entity
/// Provides predefined, reusable test data for common scenarios
/// Uses AirportBuilder for consistent object creation
/// </summary>
public static class AirportMother
{
    private static readonly AirportBuilder _defaultBuilder = new AirportBuilder();

    /// <summary>
    /// Creates a valid Airport with all required fields
    /// </summary>
    public static Airport CreateValidAirport()
    {
        return _defaultBuilder
            .WithId(1)
            .WithName("Sheremetyevo International Airport")
            .WithCity("Moscow")
            .WithCountry("Russia")
            .Build();
    }

    /// <summary>
    /// Creates an Airport with minimal data
    /// </summary>
    public static Airport CreateMinimalAirport()
    {
        return _defaultBuilder
            .WithId(2)
            .WithName("A")
            .WithCity("B")
            .WithCountry("C")
            .Build();
    }

    /// <summary>
    /// Creates Domodedovo Airport
    /// </summary>
    public static Airport CreateDomodedovoAirport()
    {
        return _defaultBuilder
            .WithId(3)
            .WithName("Domodedovo International Airport")
            .WithCity("Moscow")
            .WithCountry("Russia")
            .Build();
    }

    /// <summary>
    /// Creates Vnukovo Airport
    /// </summary>
    public static Airport CreateVnukovoAirport()
    {
        return _defaultBuilder
            .WithId(4)
            .WithName("Vnukovo International Airport")
            .WithCity("Moscow")
            .WithCountry("Russia")
            .Build();
    }

    /// <summary>
    /// Creates a list of airports for testing
    /// </summary>
    public static List<Airport> CreateAirportList(int count = 3)
    {
        var airports = new List<Airport>();
        var cities = new[] { "Moscow", "St. Petersburg", "Kazan", "Novosibirsk", "Yekaterinburg" };
        var countries = new[] { "Russia", "Russia", "Russia", "Russia", "Russia" };

        for (int i = 1; i <= count; i++)
        {
            airports.Add(_defaultBuilder
                .WithId(i)
                .WithName($"{cities[(i - 1) % cities.Length]} Airport")
                .WithCity(cities[(i - 1) % cities.Length])
                .WithCountry(countries[(i - 1) % countries.Length])
                .Build());
        }
        return airports;
    }
}
