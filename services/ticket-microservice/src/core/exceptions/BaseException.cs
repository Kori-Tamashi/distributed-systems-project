namespace core.exceptions;

/// <summary>
/// Base exception for all application exceptions
/// </summary>
public abstract class BaseException : Exception
{
    protected BaseException() : base() { }

    protected BaseException(string message) : base(message) { }

    protected BaseException(string message, Exception innerException) : base(message, innerException) { }
}
