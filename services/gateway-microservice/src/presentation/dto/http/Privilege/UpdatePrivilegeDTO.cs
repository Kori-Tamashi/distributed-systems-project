namespace presentation.dto.http.Privilege;

/// <summary>
/// DTO for updating Privilege
/// </summary>
public class UpdatePrivilegeDTO
{
    /// <summary>
    /// Username (unique) (optional)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Privilege status (BRONZE, SILVER, GOLD) (optional)
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Balance in points (optional)
    /// </summary>
    public int? Balance { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdatePrivilegeDTO()
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public UpdatePrivilegeDTO(string? username = null, int? status = null, int? balance = null)
    {
        Username = username;
        Status = status;
        Balance = balance;
    }
}
