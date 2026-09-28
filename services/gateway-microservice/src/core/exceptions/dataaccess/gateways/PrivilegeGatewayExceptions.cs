using core.exceptions.dataaccess.gateways;

namespace core.exceptions.dataaccess.gateways;

/// <summary>
/// Exception thrown when Privilege entity is not found in Bonus microservice
/// </summary>
public class PrivilegeGatewayEntityNotFoundException : GatewayEntityNotFoundException
{
    public PrivilegeGatewayEntityNotFoundException() : base()
    {
    }

    public PrivilegeGatewayEntityNotFoundException(string message) : base(message)
    {
    }

    public PrivilegeGatewayEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when communication with Bonus microservice fails
/// </summary>
public class PrivilegeGatewayCommunicationException : GatewayCommunicationException
{
    public PrivilegeGatewayCommunicationException() : base()
    {
    }

    public PrivilegeGatewayCommunicationException(string message) : base(message)
    {
    }

    public PrivilegeGatewayCommunicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when Privilege microservice request times out
/// </summary>
public class PrivilegeGatewayTimeoutException : GatewayTimeoutException
{
    public PrivilegeGatewayTimeoutException() : base()
    {
    }

    public PrivilegeGatewayTimeoutException(string message) : base(message)
    {
    }

    public PrivilegeGatewayTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Generic Privilege gateway internal server exception
/// </summary>
public class PrivilegeGatewayInternalServerException : GatewayInternalServerException
{
    public PrivilegeGatewayInternalServerException() : base()
    {
    }

    public PrivilegeGatewayInternalServerException(string message) : base(message)
    {
    }

    public PrivilegeGatewayInternalServerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
