namespace dataaccess.dto;

/// <summary>
/// Base DTO class for all data transfer objects
/// </summary>
public abstract class BaseDTO
{
    /// <summary>
    /// Unique identifier
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
    /// <param name="id">Unique identifier</param>
    protected BaseDTO(int id)
    {
        Id = id;
    }
}
