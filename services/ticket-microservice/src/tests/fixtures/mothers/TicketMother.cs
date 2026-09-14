using core.domain;
using core.enums;
using tests.fixtures.builders;

namespace tests.fixtures.mothers;

/// <summary>
/// Object Mother for Ticket entity
/// Provides predefined, reusable test data for common scenarios
/// Uses TicketBuilder for consistent object creation
/// </summary>
public static class TicketMother
{
    private static readonly TicketBuilder _defaultBuilder = new TicketBuilder();

    /// <summary>
    /// Creates a valid Ticket with all required fields
    /// </summary>
    public static Ticket CreateValidTicket()
    {
        return _defaultBuilder
            .WithId(1)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(1)
            .WithPassengerName("John Doe")
            .WithPassengerEmail("john.doe@example.com")
            .WithPassengerPhone("+79001234567")
            .WithSeatNumber("12A")
            .WithEconomyClass()
            .WithPrice(15000)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Ticket with minimal data (only required fields)
    /// </summary>
    public static Ticket CreateMinimalTicket()
    {
        return _defaultBuilder
            .WithId(2)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(1)
            .WithPassengerName("A")
            .WithPassengerEmail("a@b.com")
            .WithPassengerPhone("123")
            .WithSeatNumber("1A")
            .WithEconomyClass()
            .WithPrice(1000)
            .WithBookingDate(DateTime.UtcNow)
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Ticket with Business class
    /// </summary>
    public static Ticket CreateBusinessClassTicket()
    {
        return _defaultBuilder
            .WithId(3)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(2)
            .WithPassengerName("Jane Smith")
            .WithPassengerEmail("jane.smith@example.com")
            .WithPassengerPhone("+79009876543")
            .WithSeatNumber("2A")
            .WithBusinessClass()
            .WithPrice(45000)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Ticket with First class
    /// </summary>
    public static Ticket CreateFirstClassTicket()
    {
        return _defaultBuilder
            .WithId(4)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(3)
            .WithPassengerName("Ivan Petrov")
            .WithPassengerEmail("ivan.petrov@example.com")
            .WithPassengerPhone("+79112223344")
            .WithSeatNumber("1A")
            .WithFirstClass()
            .WithPrice(80000)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Ticket with zero price
    /// </summary>
    public static Ticket CreateTicketWithZeroPrice()
    {
        return _defaultBuilder
            .WithId(5)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(1)
            .WithPassengerName("Free Ticket")
            .WithPassengerEmail("free@example.com")
            .WithPassengerPhone("+79000000000")
            .WithSeatNumber("99Z")
            .WithEconomyClass()
            .WithZeroPrice()
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Ticket with negative price (invalid)
    /// </summary>
    public static Ticket CreateTicketWithNegativePrice()
    {
        return _defaultBuilder
            .WithId(6)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(1)
            .WithPassengerName("Invalid Ticket")
            .WithPassengerEmail("invalid@example.com")
            .WithPassengerPhone("+79000000000")
            .WithSeatNumber("99Z")
            .WithEconomyClass()
            .WithNegativePrice()
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Ticket in the past (invalid for booking)
    /// </summary>
    public static Ticket CreatePastTicket()
    {
        return _defaultBuilder
            .WithId(7)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(1)
            .WithPassengerName("Past Passenger")
            .WithPassengerEmail("past@example.com")
            .WithPassengerPhone("+79000000000")
            .WithSeatNumber("10A")
            .WithEconomyClass()
            .WithPrice(10000)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Cancelled Ticket
    /// </summary>
    public static Ticket CreateCancelledTicket()
    {
        return _defaultBuilder
            .WithId(8)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(1)
            .WithPassengerName("Cancelled Passenger")
            .WithPassengerEmail("cancelled@example.com")
            .WithPassengerPhone("+79000000000")
            .WithSeatNumber("15B")
            .WithEconomyClass()
            .WithPrice(12000)
            .WithPastBookingDate()
            .WithCancelledStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Refunded Ticket
    /// </summary>
    public static Ticket CreateRefundedTicket()
    {
        return _defaultBuilder
            .WithId(9)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(1)
            .WithPassengerName("Refunded Passenger")
            .WithPassengerEmail("refunded@example.com")
            .WithPassengerPhone("+79000000000")
            .WithSeatNumber("20C")
            .WithEconomyClass()
            .WithPrice(13000)
            .WithPastBookingDate()
            .WithRefundedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Ticket with maximum price
    /// </summary>
    public static Ticket CreateTicketWithMaxPrice()
    {
        return _defaultBuilder
            .WithId(10)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(1)
            .WithPassengerName("VIP Passenger")
            .WithPassengerEmail("vip@example.com")
            .WithPassengerPhone("+79000000000")
            .WithSeatNumber("1A")
            .WithFirstClass()
            .WithPrice(int.MaxValue)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Ticket with maximum passenger name length
    /// </summary>
    public static Ticket CreateTicketWithMaxPassengerName()
    {
        return _defaultBuilder
            .WithId(11)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(1)
            .WithPassengerName(new string('A', 255))
            .WithPassengerEmail("max@example.com")
            .WithPassengerPhone("+79000000000")
            .WithSeatNumber("5A")
            .WithEconomyClass()
            .WithPrice(15000)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a list of valid Tickets for collection tests
    /// </summary>
    public static List<Ticket> CreateTicketList(int count = 5)
    {
        var builder = new TicketBuilder();
        var tickets = builder.BuildList(count, incrementIds: true);
        // Ensure all tickets have past booking dates
        foreach (var ticket in tickets)
        {
            ticket.BookingDate = DateTime.UtcNow.AddDays(-1);
        }
        return tickets;
    }

    /// <summary>
    /// Creates a Ticket with unique random Id
    /// </summary>
    public static Ticket CreateTicketWithRandomId()
    {
        return new TicketBuilder()
            .WithPassengerName("Random Passenger")
            .WithPassengerEmail("random@example.com")
            .WithEconomyClass()
            .WithPrice(12000)
            .BuildWithRandomId();
    }
}
