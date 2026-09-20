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
    /// Creates a Ticket with minimal data
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
    /// Creates a Business Class Ticket
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
    /// Creates a First Class Ticket
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
    /// Creates a Cancelled Ticket
    /// </summary>
    public static Ticket CreateCancelledTicket()
    {
        return _defaultBuilder
            .WithId(5)
            .WithTicketUid(Guid.NewGuid())
            .WithFlightId(1)
            .WithPassengerName("Cancelled User")
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
    /// Creates a list of tickets for testing
    /// </summary>
    public static List<Ticket> CreateTicketList(int count = 5)
    {
        var tickets = new List<Ticket>();
        for (int i = 1; i <= count; i++)
        {
            tickets.Add(_defaultBuilder
                .WithId(i)
                .WithTicketUid(Guid.NewGuid())
                .WithFlightId(i % 3 + 1)
                .WithPassengerName($"Passenger {i}")
                .WithPassengerEmail($"passenger{i}@example.com")
                .WithPassengerPhone($"+7900{i:D7}")
                .WithSeatNumber($"{10 + i}A")
                .WithEconomyClass()
                .WithPrice(10000 + i * 5000)
                .WithPastBookingDate()
                .WithConfirmedStatus()
                .Build());
        }
        return tickets;
    }
}
