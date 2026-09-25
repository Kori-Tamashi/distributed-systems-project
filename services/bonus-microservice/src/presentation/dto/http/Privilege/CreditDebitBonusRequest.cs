namespace presentation.dto.http.Privilege;

/// <summary>
/// Request DTO for credit/debit bonus operations
/// </summary>
public class CreditDebitBonusRequest
{
    public int Amount { get; set; }
    public Guid TicketUid { get; set; }
}
