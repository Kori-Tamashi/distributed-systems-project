using System.ComponentModel.DataAnnotations;

namespace presentation.dto.http.Privilege;

/// <summary>
/// DTO for updating existing Privilege
/// </summary>
public class UpdatePrivilegeDTO : BaseHttpDTO
{
    /// <summary>
    /// Username (1-80 characters)
    /// </summary>
    [StringLength(80, MinimumLength = 1)]
    public string? Username { get; set; }

    /// <summary>
    /// Privilege status (BRONZE=0, SILVER=1, GOLD=2)
    /// </summary>
    [Range(0, 2)]
    public int? Status { get; set; }

    /// <summary>
    /// Balance in points (must be non-negative)
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? Balance { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdatePrivilegeDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with ID and optional fields
    /// </summary>
    public UpdatePrivilegeDTO(int id, string? username = null, int? status = null, int? balance = null)
        : base(id)
    {
        Id = id;
        Username = username;
        Status = status;
        Balance = balance;
    }
}
