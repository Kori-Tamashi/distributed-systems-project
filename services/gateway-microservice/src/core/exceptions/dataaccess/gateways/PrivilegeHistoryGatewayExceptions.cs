using core.exceptions.dataaccess.gateways;

namespace core.exceptions.dataaccess.gateways;

/// <summary>
/// Exception thrown when PrivilegeHistory entity is not found in Bonus microservice
/// </summary>
public class PrivilegeHistoryGatewayEntityNotFoundException : GatewayEntityNotFoundException
{
    public PrivilegeHistoryGatewayEntityNotFoundException() : base()
    {
    }

    public PrivilegeHistoryGatewayEntityNotFoundException(string message) : base(message)
    {
    }

    public PrivilegeHistoryGatewayEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when communication with Bonus microservice (PrivilegeHistory) fails
/// </summary>
public class PrivilegeHistoryGatewayCommunicationException : GatewayCommunicationException
{
    public PrivilegeHistoryGatewayCommunicationException() : base()
    {
    }

    public PrivilegeHistoryGatewayCommunicationException(string message) : base(message)
    {
    }

    public PrivilegeHistoryGatewayCommunicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when PrivilegeHistory microservice request times out
/// </summary>
public class PrivilegeHistoryGatewayTimeoutException : GatewayTimeoutException
{
    public PrivilegeHistoryGatewayTimeoutException() : base()
    {
    }

    public PrivilegeHistoryGatewayTimeoutException(string message) : base(message)
    {
    }

    public PrivilegeHistoryGatewayTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Generic PrivilegeHistory gateway internal server exception
/// </summary>
public class PrivilegeHistoryGatewayInternalServerException : GatewayInternalServerException
{
    public PrivilegeHistoryGatewayInternalServerException() : base()
    {
    }

    public PrivilegeHistoryGatewayInternalServerException(string message) : base(message)
    {
    }

    public PrivilegeHistoryGatewayInternalServerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
