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
    /// Sets Economy Class
    /// </summary>
    public TicketBuilder WithEconomyClass()
    {
        _class = TicketClass.Economy;
        return this;
    }

    /// <summary>
    /// Sets Business Class
    /// </summary>
    public TicketBuilder WithBusinessClass()
    {
        _class = TicketClass.Business;
        return this;
    }

    /// <summary>
    /// Sets First Class
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
    /// Sets the Booking Date
    /// </summary>
    public TicketBuilder WithBookingDate(DateTime bookingDate)
    {
        _bookingDate = bookingDate;
        return this;
    }

    /// <summary>
    /// Sets a past booking date
    /// </summary>
    public TicketBuilder WithPastBookingDate()
    {
        _bookingDate = DateTime.UtcNow.AddDays(-7);
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
    /// Sets Confirmed Status
    /// </summary>
    public TicketBuilder WithConfirmedStatus()
    {
        _status = TicketStatus.Confirmed;
        return this;
    }

    /// <summary>
    /// Sets Cancelled Status
    /// </summary>
    public TicketBuilder WithCancelledStatus()
    {
        _status = TicketStatus.Cancelled;
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
