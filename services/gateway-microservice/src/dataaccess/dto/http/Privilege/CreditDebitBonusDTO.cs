namespace dataaccess.dto.http.Privilege;

/// <summary>
/// DTO for credit/debit bonus operations
/// </summary>
public class CreditDebitBonusDTO
{
    public int Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
