using System;
using dataaccess.dto;

namespace dataaccess.dto.http;

/// <summary>
/// Base HTTP DTO class for all HTTP data transfer objects
/// Extends BaseDTO with HTTP-specific properties
/// </summary>
public abstract class BaseHttpDTO : BaseDTO
{
    /// <summary>
    /// Timestamp when the entity was created (UTC)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the entity was last updated (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

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
    protected BaseHttpDTO(int id)
        : base(id)
    {
    }
}
