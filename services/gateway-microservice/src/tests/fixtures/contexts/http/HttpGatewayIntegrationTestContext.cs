using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace tests.fixtures.contexts.http;

/// <summary>
/// Integration test context for HTTP Gateway tests
/// Provides HttpClient instances configured to connect to test API instances of other microservices
/// </summary>
public class HttpGatewayIntegrationTestContext : IDisposable
{
    private readonly Dictionary<string, HttpClient> _clients;
    private readonly Dictionary<string, string> _baseUrls;

    static HttpGatewayIntegrationTestContext()
    {
        // Load .env file to get API URLs
    }

    public HttpGatewayIntegrationTestContext()
    {
        _clients = new Dictionary<string, HttpClient>();
        _baseUrls = new Dictionary<string, string>();

        // Read base URLs from environment variables (without /api/v1 - gateways add it)
        _baseUrls["flight"] = GetEnvVar("FLIGHT_API_TEST_URL", "http://localhost:8063");
        _baseUrls["airport"] = GetEnvVar("FLIGHT_API_TEST_URL", "http://localhost:8063");
        _baseUrls["ticket"] = GetEnvVar("TICKET_API_TEST_URL", "http://localhost:8073");
        _baseUrls["booking"] = GetEnvVar("TICKET_API_TEST_URL", "http://localhost:8073");
        _baseUrls["privilege"] = GetEnvVar("BONUS_API_TEST_URL", "http://localhost:8053");
        _baseUrls["privilegehistory"] = GetEnvVar("BONUS_API_TEST_URL", "http://localhost:8053");

        // Create HttpClient instances
        foreach (var baseUrl in _baseUrls)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.Value)
            };
            _clients[baseUrl.Key] = client;
        }
    }

    /// <summary>
    /// Get HttpClient for Flight microservice
    /// </summary>
    public HttpClient GetFlightClient() => _clients["flight"];

    /// <summary>
    /// Get HttpClient for Airport (same as Flight)
    /// </summary>
    public HttpClient GetAirportClient() => _clients["airport"];

    /// <summary>
    /// Get HttpClient for Ticket microservice
    /// </summary>
    public HttpClient GetTicketClient() => _clients["ticket"];

    /// <summary>
    /// Get HttpClient for Booking (same as Ticket)
    /// </summary>
    public HttpClient GetBookingClient() => _clients["booking"];

    /// <summary>
    /// Get HttpClient for Bonus/Privilege microservice
    /// </summary>
    public HttpClient GetPrivilegeClient() => _clients["privilege"];

    /// <summary>
    /// Get HttpClient for PrivilegeHistory (same as Privilege)
    /// </summary>
    public HttpClient GetPrivilegeHistoryClient() => _clients["privilegehistory"];

    /// <summary>
    /// Get base URL for a microservice
    /// </summary>
    public string GetBaseUrl(string microservice) => _baseUrls[microservice];

    /// <summary>
    /// Get environment variable with default value
    /// </summary>
    private static string GetEnvVar(string name, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    #region IDisposable Implementation

    public void Dispose()
    {
        foreach (var client in _clients.Values)
        {
            client.Dispose();
        }
        _clients.Clear();
    }

    #endregion
}
