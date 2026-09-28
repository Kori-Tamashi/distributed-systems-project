namespace presentation.exceptions.http;

/// <summary>
/// Exception thrown when a downstream service is unavailable (503 Service Unavailable).
/// Used by Circuit Breaker and SAGA rollback scenarios.
/// </summary>
public class ServiceUnavailableException : BaseHttpException
{
    public ServiceUnavailableException(string serviceName)
        : base(503, "SERVICE_UNAVAILABLE", $"{serviceName} unavailable") { }

    public ServiceUnavailableException(string serviceName, Exception innerException)
        : base(503, "SERVICE_UNAVAILABLE", $"{serviceName} unavailable", innerException) { }
}
