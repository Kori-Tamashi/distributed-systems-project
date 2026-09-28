using core.exceptions;

namespace core.exceptions.dataaccess.gateways;

/// <summary>
/// Base exception for data access gateways (microservice communication)
/// </summary>
public abstract class BaseGatewayException : BaseException
{
    protected BaseGatewayException() : base()
    {
    }

    protected BaseGatewayException(string message) : base(message)
    {
    }

    protected BaseGatewayException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found in the target microservice
/// </summary>
public class GatewayEntityNotFoundException : BaseGatewayException
{
    public GatewayEntityNotFoundException() : base()
    {
    }

    public GatewayEntityNotFoundException(string message) : base(message)
    {
    }

    public GatewayEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when communication with target microservice fails
/// </summary>
public class GatewayCommunicationException : BaseGatewayException
{
    public GatewayCommunicationException() : base()
    {
    }

    public GatewayCommunicationException(string message) : base(message)
    {
    }

    public GatewayCommunicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when gateway request times out
/// </summary>
public class GatewayTimeoutException : BaseGatewayException
{
    public GatewayTimeoutException() : base()
    {
    }

    public GatewayTimeoutException(string message) : base(message)
    {
    }

    public GatewayTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Generic gateway internal server exception
/// </summary>
public class GatewayInternalServerException : BaseGatewayException
{
    public GatewayInternalServerException() : base()
    {
    }

    public GatewayInternalServerException(string message) : base(message)
    {
    }

    public GatewayInternalServerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
