namespace core.domain;

/// <summary>
/// Domain aggregate representing a purchased ticket with flight and privilege details
/// Used as return type for BuyTicketAsync to satisfy instructor contract
/// </summary>
public class PurchasedTicket
{
    /// <summary>
    /// The ticket entity
    /// </summary>
    public Ticket Ticket { get; set; } = null!;

    /// <summary>
    /// The flight details (loaded from FlightGateway)
    /// </summary>
    public Flight Flight { get; set; } = null!;

    /// <summary>
    /// The user's privilege (bonus account) after purchase
    /// </summary>
    public Privilege Privilege { get; set; } = null!;

    /// <summary>
    /// Amount paid using bonus balance
    /// </summary>
    public int PaidByBonuses { get; set; }

    /// <summary>
    /// Amount paid using cash/money
    /// </summary>
    public int PaidByMoney { get; set; }
}
