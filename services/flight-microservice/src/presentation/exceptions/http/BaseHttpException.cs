namespace presentation.exceptions.http;

using presentation.exceptions;

/// <summary>
/// Base HTTP exception class for all HTTP controller exceptions
/// Extends BaseException with HTTP-specific properties
/// </summary>
public abstract class BaseHttpException : BaseException
{
    /// <summary>
    /// Detailed error message for client response
    /// </summary>
    public string DetailedMessage { get; protected set; } = string.Empty;

    /// <summary>
    /// Additional error data (optional)
    /// </summary>
    public Dictionary<string, string[]>? ErrorData { get; protected set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    protected BaseHttpException()
        : base()
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Exception message</param>
    protected BaseHttpException(string message)
        : base(message)
    {
        DetailedMessage = message;
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    protected BaseHttpException(string message, Exception innerException)
        : base(message, innerException)
    {
        DetailedMessage = message;
    }

    /// <summary>
    /// Constructor with status code, error code, and message
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="errorCode">Error code for programmatic handling</param>
    /// <param name="message">Exception message</param>
    protected BaseHttpException(int statusCode, string errorCode, string message)
        : base(statusCode, errorCode, message)
    {
        DetailedMessage = message;
    }

    /// <summary>
    /// Constructor with status code, error code, message, and error data
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="errorCode">Error code for programmatic handling</param>
    /// <param name="message">Exception message</param>
    /// <param name="errorData">Additional error data</param>
    protected BaseHttpException(int statusCode, string errorCode, string message, Dictionary<string, string[]> errorData)
        : base(statusCode, errorCode, message)
    {
        DetailedMessage = message;
        ErrorData = errorData;
    }

    /// <summary>
    /// Constructor with status code, error code, message, and inner exception
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="errorCode">Error code for programmatic handling</param>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    protected BaseHttpException(int statusCode, string errorCode, string message, Exception innerException)
        : base(statusCode, errorCode, message, innerException)
    {
        DetailedMessage = message;
    }
}
