namespace dataaccess.config;

/// <summary>
/// Configuration class for microservice API URLs
/// URLs are loaded from environment variables
/// </summary>
public class MicroserviceUrls
{
    /// <summary>
    /// Flight microservice API URL (e.g., http://localhost:8060/api/v1)
    /// Environment variable: FLIGHT_URL
    /// </summary>
    public string FlightUrl { get; set; } = GetEnvVar("FLIGHT_URL", "http://localhost:8060/api/v1");

    /// <summary>
    /// Ticket microservice API URL (e.g., http://localhost:8070/api/v1)
    /// Environment variable: TICKET_URL
    /// </summary>
    public string TicketUrl { get; set; } = GetEnvVar("TICKET_URL", "http://localhost:8070/api/v1");

    /// <summary>
    /// Bonus/Privilege microservice API URL (e.g., http://localhost:8050/api/v1)
    /// Environment variable: BONUS_URL
    /// </summary>
    public string BonusUrl { get; set; } = GetEnvVar("BONUS_URL", "http://localhost:8050/api/v1");

    /// <summary>
    /// Gets environment variable or default value
    /// </summary>
    private static string GetEnvVar(string name, string defaultValue)
    {
        var envValue = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrEmpty(envValue) ? defaultValue : envValue.TrimEnd('/');
    }

    /// <summary>
    /// Reloads configuration from environment variables
    /// </summary>
    public void Reload()
    {
        FlightUrl = GetEnvVar("FLIGHT_URL", "http://localhost:8060/api/v1");
        TicketUrl = GetEnvVar("TICKET_URL", "http://localhost:8070/api/v1");
        BonusUrl = GetEnvVar("BONUS_URL", "http://localhost:8050/api/v1");
    }
}
