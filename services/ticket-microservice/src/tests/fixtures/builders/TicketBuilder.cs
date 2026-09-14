using core.domain;
using core.enums;

namespace tests.fixtures.builders;

/// <summary>
/// Test Data Builder for Ticket entity
/// Allows fluent construction of Ticket objects for testing
/// </summary>
public class TicketBuilder
{
    private int _id = 1;
    private Guid _ticketUid = Guid.NewGuid();
    private int _flightId = 1;
    private string _passengerName = "John Doe";
    private string _passengerEmail = "john.doe@example.com";
    private string _passengerPhone = "+79001234567";
    private string _seatNumber = "12A";
    private TicketClass _class = TicketClass.Economy;
    private int _price = 15000;
    private DateTime _bookingDate = DateTime.UtcNow;
    private TicketStatus _status = TicketStatus.Confirmed;

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
    /// Sets the Flight Id
    /// </summary>
    public TicketBuilder WithFlightId(int flightId)
    {
        _flightId = flightId;
        return this;
    }

    /// <summary>
    /// Sets the Passenger Name
    /// </summary>
    public TicketBuilder WithPassengerName(string passengerName)
    {
        _passengerName = passengerName;
        return this;
    }

    /// <summary>
    /// Sets the Passenger Email
    /// </summary>
    public TicketBuilder WithPassengerEmail(string passengerEmail)
    {
        _passengerEmail = passengerEmail;
        return this;
    }

    /// <summary>
    /// Sets the Passenger Phone
    /// </summary>
    public TicketBuilder WithPassengerPhone(string passengerPhone)
    {
        _passengerPhone = passengerPhone;
        return this;
    }

    /// <summary>
    /// Sets the Seat Number
    /// </summary>
    public TicketBuilder WithSeatNumber(string seatNumber)
    {
        _seatNumber = seatNumber;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Class
    /// </summary>
    public TicketBuilder WithClass(TicketClass @class)
    {
        _class = @class;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Class to Economy
    /// </summary>
    public TicketBuilder WithEconomyClass()
    {
        _class = TicketClass.Economy;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Class to Business
    /// </summary>
    public TicketBuilder WithBusinessClass()
    {
        _class = TicketClass.Business;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Class to First
    /// </summary>
    public TicketBuilder WithFirstClass()
    {
        _class = TicketClass.First;
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
    /// Sets the Booking Date
    /// </summary>
    public TicketBuilder WithBookingDate(DateTime bookingDate)
    {
        _bookingDate = bookingDate;
        return this;
    }

    /// <summary>
    /// Sets the Booking Date to future
    /// </summary>
    public TicketBuilder WithFutureBookingDate()
    {
        _bookingDate = DateTime.UtcNow.AddDays(1);
        return this;
    }

    /// <summary>
    /// Sets the Booking Date to past
    /// </summary>
    public TicketBuilder WithPastBookingDate()
    {
        _bookingDate = DateTime.UtcNow.AddDays(-1);
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
        _status = TicketStatus.Confirmed;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Status to Cancelled
    /// </summary>
    public TicketBuilder WithCancelledStatus()
    {
        _status = TicketStatus.Cancelled;
        return this;
    }

    /// <summary>
    /// Sets the Ticket Status to Refunded
    /// </summary>
    public TicketBuilder WithRefundedStatus()
    {
        _status = TicketStatus.Refunded;
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
            FlightId = _flightId,
            PassengerName = _passengerName,
            PassengerEmail = _passengerEmail,
            PassengerPhone = _passengerPhone,
            SeatNumber = _seatNumber,
            Class = _class,
            Price = _price,
            BookingDate = _bookingDate,
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
                FlightId = _flightId,
                PassengerName = $"{_passengerName} {i}",
                PassengerEmail = $"ticket{i}@example.com",
                PassengerPhone = _passengerPhone,
                SeatNumber = $"{10 + i}A",
                Class = _class,
                Price = _price,
                BookingDate = _bookingDate.AddMinutes(i),
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
            FlightId = _flightId,
            PassengerName = _passengerName,
            PassengerEmail = _passengerEmail,
            PassengerPhone = _passengerPhone,
            SeatNumber = _seatNumber,
            Class = _class,
            Price = _price,
            BookingDate = _bookingDate,
            Status = _status
        };
    }
}
