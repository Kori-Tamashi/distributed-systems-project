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
    /// Creates a minimal Airport (only required fields)
    /// </summary>
    public static Airport CreateMinimalAirport()
    {
        return _defaultBuilder
            .WithId(2)
            .WithName("Airport")
            .WithCity(string.Empty)
            .WithCountry(string.Empty)
            .Build();
    }

    /// <summary>
    /// Creates Moscow Sheremetyevo Airport
    /// </summary>
    public static Airport CreateMoscowSheremetyevo()
    {
        return _defaultBuilder
            .WithId(1)
            .WithName("Sheremetyevo International Airport")
            .WithCity("Moscow")
            .WithCountry("Russia")
            .Build();
    }

    /// <summary>
    /// Creates Moscow Domodedovo Airport
    /// </summary>
    public static Airport CreateMoscowDomodedovo()
    {
        return _defaultBuilder
            .WithId(2)
            .WithName("Domodedovo International Airport")
            .WithCity("Moscow")
            .WithCountry("Russia")
            .Build();
    }

    /// <summary>
    /// Creates Saint Petersburg Pulkovo Airport
    /// </summary>
    public static Airport CreatePetersburgPulkovo()
    {
        return _defaultBuilder
            .WithId(3)
            .WithName("Pulkovo Airport")
            .WithCity("Saint Petersburg")
            .WithCountry("Russia")
            .Build();
    }

    /// <summary>
    /// Creates Paris Charles de Gaulle Airport
    /// </summary>
    public static Airport CreateParisCDG()
    {
        return _defaultBuilder
            .WithId(4)
            .WithName("Charles de Gaulle Airport")
            .WithCity("Paris")
            .WithCountry("France")
            .Build();
    }

    /// <summary>
    /// Creates London Heathrow Airport
    /// </summary>
    public static Airport CreateLondonHeathrow()
    {
        return _defaultBuilder
            .WithId(5)
            .WithName("Heathrow Airport")
            .WithCity("London")
            .WithCountry("United Kingdom")
            .Build();
    }

    /// <summary>
    /// Creates New York JFK Airport
    /// </summary>
    public static Airport CreateNewYorkJFK()
    {
        return _defaultBuilder
            .WithId(6)
            .WithName("John F. Kennedy International Airport")
            .WithCity("New York")
            .WithCountry("United States")
            .Build();
    }

    /// <summary>
    /// Creates an Airport with empty name
    /// </summary>
    public static Airport CreateAirportWithEmptyName()
    {
        return _defaultBuilder
            .WithId(7)
            .WithName("")
            .WithCity("Test City")
            .WithCountry("Test Country")
            .Build();
    }

    /// <summary>
    /// Creates an Airport with maximum name length
    /// </summary>
    public static Airport CreateAirportWithMaxName()
    {
        return _defaultBuilder
            .WithId(8)
            .WithName(new string('A', 255))
            .WithCity("Test City")
            .WithCountry("Test Country")
            .Build();
    }

    /// <summary>
    /// Creates a list of valid Airports for collection tests
    /// </summary>
    public static List<Airport> CreateAirportList(int count = 5)
    {
        var builder = new AirportBuilder();
        return builder.BuildList(count, incrementIds: true);
    }

    /// <summary>
    /// Creates an Airport with unique random Id
    /// </summary>
    public static Airport CreateAirportWithRandomId()
    {
        return new AirportBuilder()
            .WithName("Random Airport")
            .WithCity("Random City")
            .WithCountry("Random Country")
            .BuildWithRandomId();
    }

    /// <summary>
    /// Creates Russian airports list (for testing Russian flights)
    /// </summary>
    public static List<Airport> CreateRussianAirports()
    {
        return new List<Airport>
        {
            CreateMoscowSheremetyevo(),
            CreateMoscowDomodedovo(),
            CreatePetersburgPulkovo()
        };
    }
}
