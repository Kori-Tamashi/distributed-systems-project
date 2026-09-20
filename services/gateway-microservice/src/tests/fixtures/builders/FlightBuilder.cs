using core.domain;

namespace tests.fixtures.builders;

/// <summary>
/// Test Data Builder for Flight entity
/// Allows fluent construction of Flight objects for testing
/// </summary>
public class FlightBuilder
{
    private int _id = 1;
    private string _flightNumber = "SU100";
    private Guid _flightUid = Guid.NewGuid();
    private DateTime _dateTime = DateTime.UtcNow.AddDays(1);
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
    /// Sets the Flight Number
    /// </summary>
    public FlightBuilder WithFlightNumber(string flightNumber)
    {
        _flightNumber = flightNumber;
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
    /// Sets the Flight DateTime
    /// </summary>
    public FlightBuilder WithDateTime(DateTime dateTime)
    {
        _dateTime = dateTime;
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
    /// Sets a future flight date
    /// </summary>
    public FlightBuilder WithFutureFlightDate()
    {
        _dateTime = DateTime.UtcNow.AddDays(7);
        return this;
    }

    /// <summary>
    /// Sets a past flight date
    /// </summary>
    public FlightBuilder WithPastFlightDate()
    {
        _dateTime = DateTime.UtcNow.AddDays(-1);
        return this;
    }

    /// <summary>
    /// Sets minimum price
    /// </summary>
    public FlightBuilder WithMinPrice()
    {
        _price = 1000;
        return this;
    }

    /// <summary>
    /// Sets maximum price
    /// </summary>
    public FlightBuilder WithMaxPrice()
    {
        _price = 100000;
        return this;
    }

    /// <summary>
    /// Builds the Flight object
    /// </summary>
    public Flight Build()
    {
        return new Flight
        {
            Id = _id,
            FlightNumber = _flightNumber,
            FlightUid = _flightUid,
            DateTime = _dateTime,
            FromAirportId = _fromAirportId,
            ToAirportId = _toAirportId,
            Price = _price
        };
    }
}
