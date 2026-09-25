using System.Text.Json.Serialization;

namespace presentation.dto.http;

/// <summary>
/// Standard error response for HTTP API (per lab2-template v1 spec)
/// </summary>
public class ErrorResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    public ErrorResponse(string message)
    {
        Message = message;
    }
}

/// <summary>
/// Validation error response with detailed field errors
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; }

    public ValidationErrorResponse(string message, List<string> errors)
        : base(message)
    {
        Errors = errors;
    }

    public ValidationErrorResponse(string message, params string[] errors)
        : base(message)
    {
        Errors = errors.ToList();
    }
}
