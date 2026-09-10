using core.domain;

namespace core.filters;

/// <summary>
/// Filter criteria for querying Persons from repository
/// Used for filtering and searching persons in data access layer
/// </summary>
public class PersonFilter
{
    /// <summary>
    /// Filter by name (partial match, case-insensitive)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Filter by minimum age
    /// </summary>
    public int? MinAge { get; set; }

    /// <summary>
    /// Filter by maximum age
    /// </summary>
    public int? MaxAge { get; set; }

    /// <summary>
    /// Filter by address substring (case-insensitive)
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Filter by work substring (case-insensitive)
    /// </summary>
    public string? Work { get; set; }
}
