namespace presentation.dto.http;

/// <summary>
/// Base HTTP DTO class for all data transfer objects used in HTTP API
/// Extends BaseDTO with HTTP-specific properties
/// </summary>
public abstract class BaseHttpDTO : BaseDTO
{
    /// <summary>
    /// Default constructor
    /// </summary>
    protected BaseHttpDTO()
    {
    }

    /// <summary>
    /// Constructor with ID
    /// </summary>
    /// <param name="id">Entity identifier</param>
    protected BaseHttpDTO(int id) : base(id)
    {
    }
}
