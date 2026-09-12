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
            .WithFlightUid(Guid.NewGuid())
            .WithFlightNumber("SU1234")
            .WithFutureDateTime()
            .WithFromAirportId(1)
            .WithToAirportId(2)
            .WithPrice(15000)
            .Build();
    }

    /// <summary>
    /// Creates a Flight with minimal data (only required fields)
    /// </summary>
    public static Flight CreateMinimalFlight()
    {
        return _defaultBuilder
            .WithId(2)
            .WithFlightUid(Guid.NewGuid())
            .WithFlightNumber("SU5678")
            .WithDateTime(DateTime.Now.AddHours(1))
            .WithFromAirportId(1)
            .WithToAirportId(2)
            .WithPrice(0)
            .Build();
    }

    /// <summary>
    /// Creates a Flight with zero price
    /// </summary>
    public static Flight CreateFlightWithZeroPrice()
    {
        return _defaultBuilder
            .WithId(3)
            .WithFlightUid(Guid.NewGuid())
            .WithFlightNumber("SU9999")
            .WithFutureDateTime()
            .WithFromAirportId(1)
            .WithToAirportId(2)
            .WithZeroPrice()
            .Build();
    }

    /// <summary>
    /// Creates a Flight with negative price (invalid)
    /// </summary>
    public static Flight CreateFlightWithNegativePrice()
    {
        return _defaultBuilder
            .WithId(4)
            .WithFlightUid(Guid.NewGuid())
            .WithFlightNumber("SU0000")
            .WithFutureDateTime()
            .WithFromAirportId(1)
            .WithToAirportId(2)
            .WithNegativePrice()
            .Build();
    }

    /// <summary>
    /// Creates a Flight in the past (invalid for booking)
    /// </summary>
    public static Flight CreatePastFlight()
    {
        return _defaultBuilder
            .WithId(5)
            .WithFlightUid(Guid.NewGuid())
            .WithFlightNumber("SU1111")
            .WithPastDateTime()
            .WithFromAirportId(1)
            .WithToAirportId(2)
            .WithPrice(10000)
            .Build();
    }

    /// <summary>
    /// Creates a Flight with same departure and arrival airport (invalid)
    /// </summary>
    public static Flight CreateFlightWithSameAirports()
    {
        return _defaultBuilder
            .WithId(6)
            .WithFlightUid(Guid.NewGuid())
            .WithFlightNumber("SU2222")
            .WithFutureDateTime()
            .WithFromAirportId(1)
            .WithToAirportId(1)
            .WithPrice(5000)
            .Build();
    }

    /// <summary>
    /// Creates a Flight with maximum price
    /// </summary>
    public static Flight CreateFlightWithMaxPrice()
    {
        return _defaultBuilder
            .WithId(7)
            .WithFlightUid(Guid.NewGuid())
            .WithFlightNumber("SU3333")
            .WithFutureDateTime()
            .WithFromAirportId(1)
            .WithToAirportId(2)
            .WithPrice(int.MaxValue)
            .Build();
    }

    /// <summary>
    /// Creates a Flight with maximum flight number length
    /// </summary>
    public static Flight CreateFlightWithMaxFlightNumber()
    {
        return _defaultBuilder
            .WithId(8)
            .WithFlightUid(Guid.NewGuid())
            .WithFlightNumber("ABCDEFGHIJKLMNOPQR") // 20 chars
            .WithFutureDateTime()
            .WithFromAirportId(1)
            .WithToAirportId(2)
            .WithPrice(20000)
            .Build();
    }

    /// <summary>
    /// Creates a list of valid Flights for collection tests
    /// </summary>
    public static List<Flight> CreateFlightList(int count = 5)
    {
        var builder = new FlightBuilder();
        return builder.BuildList(count, incrementIds: true);
    }

    /// <summary>
    /// Creates a Flight with unique random Id
    /// </summary>
    public static Flight CreateFlightWithRandomId()
    {
        return new FlightBuilder()
            .WithFlightNumber("SU4444")
            .WithFutureDateTime()
            .WithFromAirportId(1)
            .WithToAirportId(2)
            .WithPrice(12000)
            .BuildWithRandomId();
    }

    /// <summary>
    /// Creates a Flight for Moscow-Saint Petersburg route
    /// </summary>
    public static Flight CreateMoscowToPetersburgFlight()
    {
        return _defaultBuilder
            .WithId(10)
            .WithFlightUid(Guid.NewGuid())
            .WithFlightNumber("SU1001")
            .WithFutureDateTime()
            .WithFromAirportId(1) // Moscow
            .WithToAirportId(2) // Saint Petersburg
            .WithPrice(5000)
            .Build();
    }

    /// <summary>
    /// Creates a Flight for international route
    /// </summary>
    public static Flight CreateInternationalFlight()
    {
        return _defaultBuilder
            .WithId(11)
            .WithFlightUid(Guid.NewGuid())
            .WithFlightNumber("AF123")
            .WithFutureDateTime()
            .WithFromAirportId(1) // Moscow
            .WithToAirportId(3) // Paris
            .WithPrice(50000)
            .Build();
    }
}
