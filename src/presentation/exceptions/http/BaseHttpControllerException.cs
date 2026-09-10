using Microsoft.AspNetCore.Mvc;

namespace presentation.exceptions.http;

/// <summary>
/// Base HTTP controller exception that maps to ProblemDetails
/// Used for converting domain exceptions to HTTP responses
/// </summary>
public abstract class BaseHttpControllerException : BaseControllerException
{
    /// <summary>
    /// RFC 7807 Problem Details type URI
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// RFC 7807 Problem Details title (short human-readable title)
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// RFC 7807 Problem Details detail (human-readable explanation)
    /// </summary>
    public string Detail { get; }

    protected BaseHttpControllerException(
        string message,
        int statusCode,
        string errorCode,
        string type,
        string title,
        string detail)
        : base(message, statusCode, errorCode)
    {
        Type = type;
        Title = title;
        Detail = detail;
    }

    protected BaseHttpControllerException(
        string message,
        int statusCode,
        string errorCode,
        string type,
        string title,
        string detail,
        Exception innerException)
        : base(message, statusCode, errorCode, innerException)
    {
        Type = type;
        Title = title;
        Detail = detail;
    }

    /// <summary>
    /// Converts this exception to ProblemDetails for HTTP response
    /// </summary>
    public virtual ProblemDetails ToProblemDetails()
    {
        return new ProblemDetails
        {
            Title = Title,
            Status = StatusCode,
            Type = Type,
            Detail = Detail,
            Instance = string.Empty
        };
    }
}
