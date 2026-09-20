using core.exceptions.dataaccess.gateways;

namespace core.exceptions.dataaccess.gateways;

/// <summary>
/// Exception thrown when Airport entity is not found in Flight microservice
/// </summary>
public class AirportGatewayEntityNotFoundException : GatewayEntityNotFoundException
{
    public AirportGatewayEntityNotFoundException() : base()
    {
    }

    public AirportGatewayEntityNotFoundException(string message) : base(message)
    {
    }

    public AirportGatewayEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when communication with Flight microservice (Airport) fails
/// </summary>
public class AirportGatewayCommunicationException : GatewayCommunicationException
{
    public AirportGatewayCommunicationException() : base()
    {
    }

    public AirportGatewayCommunicationException(string message) : base(message)
    {
    }

    public AirportGatewayCommunicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when Airport microservice request times out
/// </summary>
public class AirportGatewayTimeoutException : GatewayTimeoutException
{
    public AirportGatewayTimeoutException() : base()
    {
    }

    public AirportGatewayTimeoutException(string message) : base(message)
    {
    }

    public AirportGatewayTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Generic Airport gateway internal server exception
/// </summary>
public class AirportGatewayInternalServerException : GatewayInternalServerException
{
    public AirportGatewayInternalServerException() : base()
    {
    }

    public AirportGatewayInternalServerException(string message) : base(message)
    {
    }

    public AirportGatewayInternalServerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
