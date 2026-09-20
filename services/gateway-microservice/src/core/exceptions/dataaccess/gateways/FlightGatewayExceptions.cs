using core.exceptions.dataaccess.gateways;

namespace core.exceptions.dataaccess.gateways;

/// <summary>
/// Exception thrown when Flight entity is not found in Flight microservice
/// </summary>
public class FlightGatewayEntityNotFoundException : GatewayEntityNotFoundException
{
    public FlightGatewayEntityNotFoundException() : base()
    {
    }

    public FlightGatewayEntityNotFoundException(string message) : base(message)
    {
    }

    public FlightGatewayEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when communication with Flight microservice fails
/// </summary>
public class FlightGatewayCommunicationException : GatewayCommunicationException
{
    public FlightGatewayCommunicationException() : base()
    {
    }

    public FlightGatewayCommunicationException(string message) : base(message)
    {
    }

    public FlightGatewayCommunicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when Flight microservice request times out
/// </summary>
public class FlightGatewayTimeoutException : GatewayTimeoutException
{
    public FlightGatewayTimeoutException() : base()
    {
    }

    public FlightGatewayTimeoutException(string message) : base(message)
    {
    }

    public FlightGatewayTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Generic Flight gateway internal server exception
/// </summary>
public class FlightGatewayInternalServerException : GatewayInternalServerException
{
    public FlightGatewayInternalServerException() : base()
    {
    }

    public FlightGatewayInternalServerException(string message) : base(message)
    {
    }

    public FlightGatewayInternalServerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
