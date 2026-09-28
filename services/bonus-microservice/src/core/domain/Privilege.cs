using core.enums;

using System;

namespace core.domain;

/// <summary>
/// Domain entity representing a User Privilege
/// </summary>
public class Privilege
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username (unique)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Privilege status (BRONZE, SILVER, GOLD)
    /// </summary>
    public PrivilegeStatus Status { get; set; }

    /// <summary>
    /// Balance in points
    /// </summary>
    public int Balance { get; set; }

    /// <summary>
    /// Timestamp when the privilege was created (UTC)
    /// </summary>

    /// <summary>
    /// Timestamp when the privilege was last updated (UTC)
    /// </summary>
}
