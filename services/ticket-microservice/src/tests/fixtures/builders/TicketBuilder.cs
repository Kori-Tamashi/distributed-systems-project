using core.domain;
using core.enums;

namespace tests.fixtures.builders;

/// <summary>
/// Test Data Builder for Ticket entity (per lab2-template v1 spec)
/// Table: ticket
/// Columns: ticket_uid, username, flight_number, price, status
/// </summary>
public class TicketBuilder
{
    private int _id = 1;
    private Guid _ticketUid = Guid.NewGuid();
    private string _username = "john_doe";
    private string _flightNumber = "AFL031";
    private int _price = 15000;
    private TicketStatus _status = TicketStatus.Paid;

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
    /// Sets the Ticket Price to zero
    /// </summary>
    public TicketBuilder WithZeroPrice()
    {
        _price = 0;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Price to negative (invalid)
    /// </summary>
    public TicketBuilder WithNegativePrice()
    {
        _price = -1000;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Status
    /// </summary>
    public TicketBuilder WithStatus(TicketStatus status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Status to Confirmed
    /// </summary>
    public TicketBuilder WithConfirmedStatus()
    {
        _status = TicketStatus.Paid;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Status to Cancelled
    /// </summary>
    public TicketBuilder WithCancelledStatus()
    {
        _status = TicketStatus.Canceled;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Status to Refunded
    /// </summary>
    public TicketBuilder WithRefundedStatus()
    {
        // Refunded status removed - not in lab2-template v1 API
        _status = TicketStatus.Canceled;
        return this;
    }

    /// <summary>
    /// Builds the Ticket object with current configuration
    /// </summary>
    /// <returns>Ticket entity with configured properties</returns>
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

    /// <summary>
    /// Builds a list of Ticket objects
    /// </summary>
    /// <param name="count">Number of tickets to build</param>
    /// <param name="incrementIds">Whether to increment Ids for each ticket</param>
    /// <returns>List of Ticket entities</returns>
    public List<Ticket> BuildList(int count, bool incrementIds = true)
    {
        var tickets = new List<Ticket>();
        
        for (int i = 0; i < count; i++)
        {
            var ticket = new Ticket
            {
                Id = incrementIds ? _id + i : _id,
                TicketUid = Guid.NewGuid(),
                Username = $"{_username}_{i}",
                FlightNumber = _flightNumber,
                Price = _price,
                Status = _status
            };
            tickets.Add(ticket);
        }

        return tickets;
    }

    /// <summary>
    /// Creates a Ticket with random Id
    /// </summary>
    public Ticket BuildWithRandomId()
    {
        return new Ticket
        {
            Id = new Random().Next(1, int.MaxValue),
            TicketUid = Guid.NewGuid(),
            Username = _username,
            FlightNumber = _flightNumber,
            Price = _price,
            Status = _status
        };
    }
}
