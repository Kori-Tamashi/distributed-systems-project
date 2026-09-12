using core.domain;

namespace tests.fixtures.builders;

/// <summary>
/// Test Data Builder for Flight entity
/// Allows fluent construction of Flight objects for testing
/// </summary>
public class FlightBuilder
{
    private int _id = 1;
    private Guid _flightUid = Guid.NewGuid();
    private string _flightNumber = "SU1234";
    private DateTime _dateTime = DateTime.Now.AddHours(2);
    private int _fromAirportId = 1;
    private int _toAirportId = 2;
    private int _price = 15000;

    /// <summary>
    /// Sets the Flight Id
    /// </summary>
    public FlightBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the Flight UID
    /// </summary>
    public FlightBuilder WithFlightUid(Guid flightUid)
    {
        _flightUid = flightUid;
        return this;
    }

    /// <summary>
    /// Sets the Flight Number
    /// </summary>
    public FlightBuilder WithFlightNumber(string flightNumber)
    {
        _flightNumber = flightNumber;
        return this;
    }

    /// <summary>
    /// Sets the Flight DateTime
    /// </summary>
    public FlightBuilder WithDateTime(DateTime dateTime)
    {
        _dateTime = dateTime;
        return this;
    }

    /// <summary>
    /// Sets the Flight DateTime to future
    /// </summary>
    public FlightBuilder WithFutureDateTime()
    {
        _dateTime = DateTime.Now.AddDays(1);
        return this;
    }

    /// <summary>
    /// Sets the Flight DateTime to past
    /// </summary>
    public FlightBuilder WithPastDateTime()
    {
        _dateTime = DateTime.Now.AddDays(-1);
        return this;
    }

    /// <summary>
    /// Sets the From Airport Id
    /// </summary>
    public FlightBuilder WithFromAirportId(int fromAirportId)
    {
        _fromAirportId = fromAirportId;
        return this;
    }

    /// <summary>
    /// Sets the To Airport Id
    /// </summary>
    public FlightBuilder WithToAirportId(int toAirportId)
    {
        _toAirportId = toAirportId;
        return this;
    }

    /// <summary>
    /// Sets the Flight Price
    /// </summary>
    public FlightBuilder WithPrice(int price)
    {
        _price = price;
        return this;
    }

    /// <summary>
    /// Sets the Flight Price to zero
    /// </summary>
    public FlightBuilder WithZeroPrice()
    {
        _price = 0;
        return this;
    }

    /// <summary>
    /// Sets the Flight Price to negative (invalid)
    /// </summary>
    public FlightBuilder WithNegativePrice()
    {
        _price = -1000;
        return this;
    }

    /// <summary>
    /// Builds the Flight object with current configuration
    /// </summary>
    /// <returns>Flight entity with configured properties</returns>
    public Flight Build()
    {
        return new Flight
        {
            Id = _id,
            FlightUid = _flightUid,
            FlightNumber = _flightNumber,
            DateTime = _dateTime,
            FromAirportId = _fromAirportId,
            ToAirportId = _toAirportId,
            Price = _price
        };
    }

    /// <summary>
    /// Builds a list of Flight objects
    /// </summary>
    /// <param name="count">Number of flights to build</param>
    /// <param name="incrementIds">Whether to increment Ids for each flight</param>
    /// <returns>List of Flight entities</returns>
    public List<Flight> BuildList(int count, bool incrementIds = true)
    {
        var flights = new List<Flight>();
        
        for (int i = 0; i < count; i++)
        {
            var flight = new Flight
            {
                Id = incrementIds ? _id + i : _id,
                FlightUid = Guid.NewGuid(),
                FlightNumber = $"{_flightNumber}{i}",
                DateTime = _dateTime.AddHours(i),
                FromAirportId = _fromAirportId,
                ToAirportId = _toAirportId,
                Price = _price
            };
            flights.Add(flight);
        }

        return flights;
    }

    /// <summary>
    /// Creates a Flight with random Id
    /// </summary>
    public Flight BuildWithRandomId()
    {
        return new Flight
        {
            Id = new Random().Next(1, int.MaxValue),
            FlightUid = Guid.NewGuid(),
            FlightNumber = _flightNumber,
            DateTime = _dateTime,
            FromAirportId = _fromAirportId,
            ToAirportId = _toAirportId,
            Price = _price
        };
    }
}
