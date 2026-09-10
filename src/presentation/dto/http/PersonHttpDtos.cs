using Microsoft.AspNetCore.Mvc;

namespace presentation.dto.http;

/// <summary>
/// Base DTO for Person HTTP responses
/// Contains common Person properties
/// </summary>
public abstract class BasePersonHttpDto : BaseHttpDto
{
    /// <summary>
    /// Person's full name
    /// </summary>
    public string? Name { get; set; } = string.Empty;

    /// <summary>
    /// Person's age
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// Person's address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Person's work information
    /// </summary>
    public string? Work { get; set; }
}

/// <summary>
/// HTTP Request DTO for creating a new Person
/// </summary>
public class CreatePersonHttpDto : BasePersonHttpDto
{
}

/// <summary>
/// HTTP Request DTO for updating an existing Person
/// </summary>
public class UpdatePersonHttpDto : BasePersonHttpDto
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public new int Id { get; set; }
}

/// <summary>
/// HTTP Response DTO for Person
/// </summary>
public class PersonHttpDto : BasePersonHttpDto
{
    /// <summary>
    /// Person's unique identifier
    /// </summary>
    public new int Id { get; set; }
}

/// <summary>
/// HTTP Response DTO for Person list with pagination
/// </summary>
public class PersonListHttpDto : BasePagedHttpDto<PersonHttpDto>
{
}

/// <summary>
/// HTTP Response DTO for Person created (201 Created)
/// </summary>
public class PersonCreatedHttpDto : BasePersonHttpDto
{
    /// <summary>
    /// Location header URI for the created resource
    /// </summary>
    public string? Location { get; set; }

    public PersonCreatedHttpDto()
    {
        StatusCode = StatusCodes.Status201Created;
    }
}

/// <summary>
/// HTTP Response DTO for Person not found (404)
/// </summary>
public class PersonNotFoundHttpDto : BasePersonHttpDto
{
    public int PersonId { get; set; }
    public string Message { get; set; } = string.Empty;

    public PersonNotFoundHttpDto()
    {
        StatusCode = StatusCodes.Status404NotFound;
    }
}

/// <summary>
/// HTTP Response DTO for validation error (400)
/// </summary>
public class PersonValidationHttpDto : BasePersonHttpDto
{
    public Dictionary<string, string[]> Errors { get; set; } = new();
    public string Message { get; set; } = string.Empty;

    public PersonValidationHttpDto()
    {
        StatusCode = StatusCodes.Status400BadRequest;
    }
}

/// <summary>
/// HTTP Response DTO for person already exists (409)
/// </summary>
public class PersonAlreadyExistsHttpDto : BasePersonHttpDto
{
    public int PersonId { get; set; }
    public string Message { get; set; } = string.Empty;

    public PersonAlreadyExistsHttpDto()
    {
        StatusCode = StatusCodes.Status409Conflict;
    }
    
    public PersonAlreadyExistsHttpDto(int personId, string message)
    {
        PersonId = personId;
        Message = message;
        StatusCode = StatusCodes.Status409Conflict;
    }
}
