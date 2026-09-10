namespace presentation.dto;

/// <summary>
/// Base interface for all DTOs
/// </summary>
public interface IBaseDto
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    int Id { get; set; }
}

/// <summary>
/// Base DTO class with common properties
/// </summary>
public abstract class BaseDto : IBaseDto
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public int Id { get; set; }
}
