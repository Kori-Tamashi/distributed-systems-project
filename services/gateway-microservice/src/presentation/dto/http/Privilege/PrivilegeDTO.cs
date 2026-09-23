using presentation.dto.http;

namespace presentation.dto.http.Privilege;

/// <summary>
/// DTO for reading Privilege data (full representation)
/// </summary>
public class PrivilegeDTO : BaseHttpDTO
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
    public PrivilegeDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public PrivilegeDTO(
        int id,
        string username,
        int status,
        int balance)
        : base(id)
    {
        Id = id;
        Username = username;
        Status = status;
        Balance = balance;
    }
}
