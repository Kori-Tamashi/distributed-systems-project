using System;

namespace presentation.dto.http;

using presentation.dto;

/// <summary>
/// Base HTTP DTO class for all HTTP data transfer objects
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
    protected BaseHttpDTO(int id)
        : base(id)
    {
    }
}
