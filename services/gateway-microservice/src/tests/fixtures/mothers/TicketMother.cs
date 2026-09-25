using core.domain;
using tests.fixtures.builders;

namespace tests.fixtures.mothers;

/// <summary>
/// Object Mother for Ticket entity (per lab2-template v1 spec)
/// Provides predefined, reusable test data for common scenarios
/// Uses TicketBuilder for consistent object creation
/// 
/// NEW Ticket model fields:
/// - int Id
/// - Guid TicketUid
/// - string Username
/// - string FlightNumber
/// - int Price
/// - int Status (PAID=1, CANCELED=2)
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
            .WithPaidStatus()
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
            .WithUsername("a")
            .WithFlightNumber("AFL001")
            .WithPrice(1000)
            .WithPaidStatus()
            .Build();
    }

    /// <summary>
    /// Creates a high-price ticket
    /// </summary>
    public static Ticket CreateHighPriceTicket()
    {
        return _defaultBuilder
            .WithId(3)
            .WithTicketUid(Guid.NewGuid())
            .WithUsername("jane_smith")
            .WithFlightNumber("AFL100")
            .WithPrice(80000)
            .WithPaidStatus()
            .Build();
    }

    /// <summary>
    /// Creates a low-price ticket
    /// </summary>
    public static Ticket CreateLowPriceTicket()
    {
        return _defaultBuilder
            .WithId(4)
            .WithTicketUid(Guid.NewGuid())
            .WithUsername("budget_traveler")
            .WithFlightNumber("AFL005")
            .WithPrice(5000)
            .WithPaidStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Cancelled ticket
    /// </summary>
    public static Ticket CreateCancelledTicket()
    {
        return _defaultBuilder
            .WithId(5)
            .WithTicketUid(Guid.NewGuid())
            .WithUsername("cancelled_user")
            .WithFlightNumber("AFL032")
            .WithPrice(12000)
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
                .WithUsername($"user_{i}")
                .WithFlightNumber($"AFL{100 + i}")
                .WithPrice(10000 + i * 5000)
                .WithPaidStatus()
                .Build());
        }
        return tickets;
    }

    /// <summary>
    /// Creates a list of tickets for a specific user
    /// </summary>
    public static List<Ticket> CreateTicketListForUser(string username, int count = 3)
    {
        var tickets = new List<Ticket>();
        for (int i = 1; i <= count; i++)
        {
            tickets.Add(_defaultBuilder
                .WithId(i)
                .WithTicketUid(Guid.NewGuid())
                .WithUsername(username)
                .WithFlightNumber($"AFL{100 + i}")
                .WithPrice(10000 + i * 5000)
                .WithPaidStatus()
                .Build());
        }
        return tickets;
    }
}
