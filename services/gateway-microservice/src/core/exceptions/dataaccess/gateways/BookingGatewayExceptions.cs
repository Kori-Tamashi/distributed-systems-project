using core.exceptions.dataaccess.gateways;

namespace core.exceptions.dataaccess.gateways;

/// <summary>
/// Exception thrown when Booking entity is not found in Ticket microservice
/// </summary>
public class BookingGatewayEntityNotFoundException : GatewayEntityNotFoundException
{
    public BookingGatewayEntityNotFoundException() : base()
    {
    }

    public BookingGatewayEntityNotFoundException(string message) : base(message)
    {
    }

    public BookingGatewayEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when communication with Ticket microservice (Booking) fails
/// </summary>
public class BookingGatewayCommunicationException : GatewayCommunicationException
{
    public BookingGatewayCommunicationException() : base()
    {
    }

    public BookingGatewayCommunicationException(string message) : base(message)
    {
    }

    public BookingGatewayCommunicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when Booking microservice request times out
/// </summary>
public class BookingGatewayTimeoutException : GatewayTimeoutException
{
    public BookingGatewayTimeoutException() : base()
    {
    }

    public BookingGatewayTimeoutException(string message) : base(message)
    {
    }

    public BookingGatewayTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Generic Booking gateway internal server exception
/// </summary>
public class BookingGatewayInternalServerException : GatewayInternalServerException
{
    public BookingGatewayInternalServerException() : base()
    {
    }

    public BookingGatewayInternalServerException(string message) : base(message)
    {
    }

    public BookingGatewayInternalServerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
