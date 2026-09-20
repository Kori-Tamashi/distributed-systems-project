namespace core.exceptions;

/// <summary>
/// Base exception class for all exceptions in the system
/// </summary>
public abstract class BaseException : Exception
{
    /// <summary>
    /// Default constructor
    /// </summary>
    protected BaseException() : base()
    {
    }

    /// <summary>
    /// Constructor with message
    /// </summary>
    /// <param name="message">Exception message</param>
    protected BaseException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructor with message and inner exception
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    protected BaseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
