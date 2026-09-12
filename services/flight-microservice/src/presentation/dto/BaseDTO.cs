namespace presentation.dto;

/// <summary>
/// Base DTO class for all data transfer objects
/// Provides common properties and functionality for all DTOs
/// </summary>
public abstract class BaseDTO
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    protected BaseDTO()
    {
    }

    /// <summary>
    /// Constructor with ID
    /// </summary>
    /// <param name="id">Entity identifier</param>
    protected BaseDTO(int id)
    {
        Id = id;
    }
}
