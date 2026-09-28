using core.domain;
using core.enums;
using tests.fixtures.builders;

namespace tests.fixtures.mothers;

/// <summary>
/// Object Mother for Ticket entity (per lab2-template v1 spec)
/// Table: ticket
/// Columns: ticket_uid, username, flight_number, price, status
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
            .WithUsername("john_doe")
            .WithFlightNumber("AFL031")
            .WithPrice(15000)
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
            .WithUsername("a")
            .WithFlightNumber("AFL001")
            .WithPrice(1000)
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Ticket with high price
    /// </summary>
    public static Ticket CreateBusinessClassTicket()
    {
        return _defaultBuilder
            .WithId(3)
            .WithTicketUid(Guid.NewGuid())
            .WithUsername("jane_smith")
            .WithFlightNumber("AFL032")
            .WithPrice(45000)
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Ticket with maximum price
    /// </summary>
    public static Ticket CreateFirstClassTicket()
    {
        return _defaultBuilder
            .WithId(4)
            .WithTicketUid(Guid.NewGuid())
            .WithUsername("ivan_petrov")
            .WithFlightNumber("AFL033")
            .WithPrice(80000)
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
            .WithUsername("free_ticket")
            .WithFlightNumber("AFL031")
            .WithZeroPrice()
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
            .WithUsername("invalid_ticket")
            .WithFlightNumber("AFL031")
            .WithNegativePrice()
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
            .WithUsername("cancelled_passenger")
            .WithFlightNumber("AFL031")
            .WithPrice(12000)
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
            .WithUsername("refunded_passenger")
            .WithFlightNumber("AFL031")
            .WithPrice(13000)
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
            .WithUsername("vip_passenger")
            .WithFlightNumber("AFL031")
            .WithPrice(int.MaxValue)
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Ticket with maximum username length
    /// </summary>
    public static Ticket CreateTicketWithMaxUsername()
    {
        return _defaultBuilder
            .WithId(11)
            .WithTicketUid(Guid.NewGuid())
            .WithUsername(new string('A', 80))
            .WithFlightNumber("AFL031")
            .WithPrice(15000)
            .WithConfirmedStatus()
            .Build();
    }

    /// <summary>
    /// Creates a list of valid Tickets for collection tests
    /// </summary>
    public static List<Ticket> CreateTicketList(int count = 5)
    {
        var builder = new TicketBuilder();
        return builder.BuildList(count, incrementIds: true);
    }

    /// <summary>
    /// Creates a Ticket with unique random Id
    /// </summary>
    public static Ticket CreateTicketWithRandomId()
    {
        return new TicketBuilder()
            .WithUsername("random_passenger")
            .BuildWithRandomId();
    }
}
