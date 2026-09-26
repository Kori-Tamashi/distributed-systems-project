using presentation.dto.http.Ticket;
using System.Text.Json.Serialization;

namespace presentation.dto.http.User;

/// <summary>
/// DTO for reading complete user information including tickets and privilege status
/// </summary>
public class UserInfoDTO
{
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Privilege information (bonus account status)
    /// </summary>
    [JsonPropertyName("privilege")]
    public PrivilegeInfoDTO? PrivilegeInfo { get; set; }

    /// <summary>
    /// List of all user tickets
    /// </summary>
    public List<TicketDTO> Tickets { get; set; } = new();

    /// <summary>
    /// Default constructor
    /// </summary>
    public UserInfoDTO()
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public UserInfoDTO(
        string username,
        PrivilegeInfoDTO? privilegeInfo,
        List<TicketDTO> tickets)
    {
        Username = username;
        PrivilegeInfo = privilegeInfo;
        Tickets = tickets ?? new List<TicketDTO>();
    }
}

/// <summary>
/// DTO for privilege information
/// </summary>
public class PrivilegeInfoDTO
{
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Privilege status (BRONZE, SILVER, GOLD)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Bonus balance in points
    /// </summary>
    public int Balance { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public PrivilegeInfoDTO()
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public PrivilegeInfoDTO(string username, string status, int balance)
    {
        Username = username;
        Status = status;
        Balance = balance;
    }
}
