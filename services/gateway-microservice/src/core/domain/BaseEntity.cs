namespace core.domain;

/// <summary>
/// Base class for all domain entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }
}
