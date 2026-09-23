using core.domain;
using tests.fixtures.builders;

namespace tests.fixtures.mothers;

/// <summary>
/// Object Mother for Flight entity
/// Provides predefined, reusable test data for common scenarios
/// Uses FlightBuilder for consistent object creation
/// </summary>
public static class FlightMother
{
    private static readonly FlightBuilder _defaultBuilder = new FlightBuilder();

    /// <summary>
    /// Creates a valid Flight with all required fields
    /// </summary>
    public static Flight CreateValidFlight()
    {
        return _defaultBuilder
            .WithId(1)
            .WithFlightNumber("SU100")
            .WithFlightUid(Guid.NewGuid())
            .WithFromAirportId(1)
            .WithToAirportId(2)
            .WithPrice(15000)
            .WithDateTime(DateTime.UtcNow.AddHours(1))
            .Build();
    }

    /// <summary>
    /// Creates a Flight with minimal data
    /// </summary>
    public static Flight CreateMinimalFlight()
    {
        return _defaultBuilder
            .WithId(2)
            .WithFlightNumber("AF200")
            .WithFlightUid(Guid.NewGuid())
            .WithFromAirportId(1)
            .WithToAirportId(3)
            .WithMinPrice()
            .WithDateTime(DateTime.UtcNow)
            .Build();
    }

    /// <summary>
    /// Creates a Flight with maximum price
    /// </summary>
    public static Flight CreateExpensiveFlight()
    {
        return _defaultBuilder
            .WithId(3)
            .WithFlightNumber("LH300")
            .WithFlightUid(Guid.NewGuid())
            .WithFromAirportId(2)
            .WithToAirportId(4)
            .WithMaxPrice()
            .WithDateTime(DateTime.UtcNow.AddHours(-1))
            .Build();
    }

    /// <summary>
    /// Creates a Flight with zero price
    /// </summary>
    public static Flight CreateFreeFlight()
    {
        return _defaultBuilder
            .WithId(4)
            .WithFlightNumber("FREE1")
            .WithFlightUid(Guid.NewGuid())
            .WithFromAirportId(1)
            .WithToAirportId(2)
            .WithPrice(0)
            .WithDateTime(DateTime.UtcNow.AddHours(-1))
            .Build();
    }

    /// <summary>
    /// Creates a list of flights for testing
    /// </summary>
    public static List<Flight> CreateFlightList(int count = 5)
    {
        var flights = new List<Flight>();
        for (int i = 1; i <= count; i++)
        {
            flights.Add(_defaultBuilder
                .WithId(i)
                .WithFlightNumber($"FL{i:D3}")
                .WithFlightUid(Guid.NewGuid())
                .WithFromAirportId(i % 3 + 1)
                .WithToAirportId((i + 1) % 3 + 1)
                .WithPrice(10000 + i * 5000)
                .WithDateTime(DateTime.UtcNow.AddHours(-1))
                .Build());
        }
        return flights;
    }
}
