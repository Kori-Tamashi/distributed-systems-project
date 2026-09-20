using System.ComponentModel.DataAnnotations;
using dataaccess.dto.http;

namespace dataaccess.dto.http.Privilege;

/// <summary>
/// DTO for creating new Privilege
/// </summary>
public class CreatePrivilegeDTO : BaseHttpDTO
{
    /// <summary>
    /// Username (required, 1-80 characters)
    /// </summary>
    [Required]
    [StringLength(80, MinimumLength = 1)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Privilege status (BRONZE=0, SILVER=1, GOLD=2)
    /// </summary>
    [Range(0, 2)]
    public int Status { get; set; }

    /// <summary>
    /// Initial balance in points (default: 0)
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Balance { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CreatePrivilegeDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with required fields
    /// </summary>
    public CreatePrivilegeDTO(string username, int status, int balance = 0)
        : base(0)
    {
        Username = username;
        Status = status;
        Balance = balance;
    }
}
