using core.exceptions.dataaccess.gateways;

namespace core.exceptions.dataaccess.gateways;

/// <summary>
/// Exception thrown when Ticket entity is not found in Ticket microservice
/// </summary>
public class TicketGatewayEntityNotFoundException : GatewayEntityNotFoundException
{
    public TicketGatewayEntityNotFoundException() : base()
    {
    }

    public TicketGatewayEntityNotFoundException(string message) : base(message)
    {
    }

    public TicketGatewayEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when communication with Ticket microservice fails
/// </summary>
public class TicketGatewayCommunicationException : GatewayCommunicationException
{
    public TicketGatewayCommunicationException() : base()
    {
    }

    public TicketGatewayCommunicationException(string message) : base(message)
    {
    }

    public TicketGatewayCommunicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when Ticket microservice request times out
/// </summary>
public class TicketGatewayTimeoutException : GatewayTimeoutException
{
    public TicketGatewayTimeoutException() : base()
    {
    }

    public TicketGatewayTimeoutException(string message) : base(message)
    {
    }

    public TicketGatewayTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Generic Ticket gateway internal server exception
/// </summary>
public class TicketGatewayInternalServerException : GatewayInternalServerException
{
    public TicketGatewayInternalServerException() : base()
    {
    }

    public TicketGatewayInternalServerException(string message) : base(message)
    {
    }

    public TicketGatewayInternalServerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
