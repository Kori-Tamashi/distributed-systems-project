using System.Collections.Generic;
using dataaccess.dto.http.PrivilegeHistory;

namespace dataaccess.dto.http.Privilege;

/// <summary>
/// DTO for getting privilege with history by username
/// </summary>
public class PrivilegeWithHistoryDTO
{
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Balance
    /// </summary>
    public int Balance { get; set; }

    /// <summary>
    /// Status (BRONZE/SILVER/GOLD)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Privilege history
    /// </summary>
    public List<PrivilegeHistoryDTO> History { get; set; } = new();
}
