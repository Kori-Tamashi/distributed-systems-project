using core.exceptions;

namespace core.exceptions.businesslogic.services;

/// <summary>
/// Exception thrown when a downstream service is unavailable.
/// Used by SAGA rollback scenarios. Maps to HTTP 503 in presentation layer.
/// </summary>
public class ServiceUnavailableException : BaseServiceException
{
    public string ServiceName { get; }

    public ServiceUnavailableException(string serviceName)
        : base($"{serviceName} unavailable")
    {
        ServiceName = serviceName;
    }

    public ServiceUnavailableException(string serviceName, Exception innerException)
        : base($"{serviceName} unavailable", innerException)
    {
        ServiceName = serviceName;
    }
}
