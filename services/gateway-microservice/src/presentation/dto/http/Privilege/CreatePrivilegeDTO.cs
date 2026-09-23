namespace presentation.dto.http.Privilege;

/// <summary>
/// DTO for creating Privilege
/// </summary>
public class CreatePrivilegeDTO
{
    /// <summary>
    /// Username (unique)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Privilege status (BRONZE, SILVER, GOLD)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Balance in points
    /// </summary>
    public int Balance { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CreatePrivilegeDTO()
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public CreatePrivilegeDTO(string username, int status, int balance)
    {
        Username = username;
        Status = status;
        Balance = balance;
    }
}
