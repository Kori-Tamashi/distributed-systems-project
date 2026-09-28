using core.domain;

namespace tests.fixtures.builders;

/// <summary>
/// Test Data Builder for Ticket entity (per lab2-template v1 spec)
/// Allows fluent construction of Ticket objects for testing
/// </summary>
public class TicketBuilder
{
    private int _id = 1;
    private Guid _ticketUid = Guid.NewGuid();
    private string _username = "john_doe";
    private string _flightNumber = "AFL031";
    private int _price = 15000;
    private int _status = 1; // PAID

    /// <summary>
    /// Sets the Ticket Id
    /// </summary>
    public TicketBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the Ticket UID
    /// </summary>
    public TicketBuilder WithTicketUid(Guid ticketUid)
    {
        _ticketUid = ticketUid;
        return this;
    }

    /// <summary>
    /// Sets the Username
    /// </summary>
    public TicketBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    /// <summary>
    /// Sets the Flight Number
    /// </summary>
    public TicketBuilder WithFlightNumber(string flightNumber)
    {
        _flightNumber = flightNumber;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Price
    /// </summary>
    public TicketBuilder WithPrice(int price)
    {
        _price = price;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Status (PAID=1, CANCELED=2)
    /// </summary>
    public TicketBuilder WithStatus(int status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets PAID status
    /// </summary>
    public TicketBuilder WithPaidStatus()
    {
        _status = 1;
        return this;
    }

    /// <summary>
    /// Sets CANCELED status
    /// </summary>
    public TicketBuilder WithCancelledStatus()
    {
        _status = 2;
        return this;
    }

    /// <summary>
    /// Builds the Ticket object
    /// </summary>
    public Ticket Build()
    {
        return new Ticket
        {
            Id = _id,
            TicketUid = _ticketUid,
            Username = _username,
            FlightNumber = _flightNumber,
            Price = _price,
            Status = _status
        };
    }
}
