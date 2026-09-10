namespace presentation.exceptions;

/// <summary>
/// Base exception for controller-level errors
/// Used to wrap service/repository exceptions for HTTP responses
/// </summary>
public abstract class BaseControllerException : Exception
{
    /// <summary>
    /// HTTP status code associated with this exception
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Error code for client consumption
    /// </summary>
    public string ErrorCode { get; }

    protected BaseControllerException(string message, int statusCode, string errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    protected BaseControllerException(string message, int statusCode, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
