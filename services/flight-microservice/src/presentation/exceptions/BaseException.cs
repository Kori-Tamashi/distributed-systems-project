namespace presentation.exceptions;

/// <summary>
/// Base exception class for all presentation layer exceptions
/// Provides common functionality for HTTP controller exceptions
/// </summary>
public abstract class BaseException : Exception
{
    /// <summary>
    /// HTTP status code associated with this exception
    /// </summary>
    public int StatusCode { get; protected set; }

    /// <summary>
    /// Error code for programmatic handling
    /// </summary>
    public string ErrorCode { get; protected set; } = string.Empty;

    /// <summary>
    /// Default constructor
    /// </summary>
    protected BaseException()
        : base()
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Exception message</param>
    protected BaseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    protected BaseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Constructor with status code, error code, and message
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="errorCode">Error code for programmatic handling</param>
    /// <param name="message">Exception message</param>
    protected BaseException(int statusCode, string errorCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Constructor with status code, error code, message, and inner exception
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="errorCode">Error code for programmatic handling</param>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    protected BaseException(int statusCode, string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
