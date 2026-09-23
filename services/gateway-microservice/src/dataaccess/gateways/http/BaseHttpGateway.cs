using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using core.exceptions.dataaccess.gateways;
using Microsoft.Extensions.Logging;

namespace dataaccess.gateways.http;

/// <summary>
/// Base class for HTTP gateways with common functionality
/// Implements best practices for microservice communication:
/// - Centralized error handling
/// - Logging
/// - Retry logic support
/// - Timeout handling
/// - JSON serialization configuration
/// </summary>
public abstract class BaseHttpGateway
{
    protected readonly HttpClient HttpClient;
    protected readonly JsonSerializerOptions JsonOptions;
    protected readonly string BaseUrl;
    protected readonly ILogger? Logger;

    /// <summary>
    /// Initializes a new instance of the BaseHttpGateway
    /// </summary>
    /// <param name="httpClient">HttpClient for making HTTP requests (configured with base address and timeouts)</param>
    /// <param name="baseUrl">Base URL of the target microservice API (e.g., http://localhost:8060/api/v1)</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    protected BaseHttpGateway(HttpClient httpClient, string baseUrl, ILogger? logger = null)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        BaseUrl = baseUrl.TrimEnd('/');
        Logger = logger;
        
        // Configure JSON options for serialization
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Log initialization
        Logger?.LogInformation("Gateway initialized for {BaseUrl}", BaseUrl);
    }

    /// <summary>
    /// Handles HTTP response and throws appropriate exceptions
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="response">HTTP response</param>
    /// <param name="operation">Operation name (GET/POST/PUT/DELETE)</param>
    /// <param name="entityId">Optional entity ID for error messages</param>
    /// <returns>Deserialized response</returns>
    /// <exception cref="GatewayEntityNotFoundException">When entity is not found</exception>
    /// <exception cref="GatewayCommunicationException">When communication fails</exception>
    /// <exception cref="GatewayTimeoutException">When request times out</exception>
    /// <exception cref="GatewayInternalServerException">When server error occurs</exception>
    protected async Task<T> HandleResponseAsync<T>(HttpResponseMessage response, string operation, int? entityId = null)
    {
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        // Success cases
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Created)
        {
            Logger?.LogDebug("{Operation} {Url} returned {StatusCode}", operation, response.RequestMessage?.RequestUri, response.StatusCode);
            
            if (typeof(T) == typeof(bool))
                return (T)(object)true;
            
            if (content == "[]") return default!; return string.IsNullOrEmpty(content) ? default! : JsonSerializer.Deserialize<T>(content, JsonOptions)!;
        }

        // Not found
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            Logger?.LogWarning("{Operation} {Url} returned 404 Not Found. EntityId: {EntityId}", operation, response.RequestMessage?.RequestUri, entityId);
            var entityName = GetEntityName<T>();
            throw CreateNotFoundException(entityName, entityId);
        }

        // Bad request
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            Logger?.LogWarning("{Operation} {Url} returned 400 Bad Request: {Content}", operation, response.RequestMessage?.RequestUri, content);
            throw new GatewayCommunicationException($"Bad request: {content}");
        }

        // Unauthorized
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            Logger?.LogWarning("{Operation} {Url} returned 401 Unauthorized", operation, response.RequestMessage?.RequestUri);
            throw new GatewayCommunicationException("Unauthorized access to the service");
        }

        // Forbidden
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            Logger?.LogWarning("{Operation} {Url} returned 403 Forbidden", operation, response.RequestMessage?.RequestUri);
            throw new GatewayCommunicationException("Access forbidden");
        }

        // Conflict
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            Logger?.LogWarning("{Operation} {Url} returned 409 Conflict: {Content}", operation, response.RequestMessage?.RequestUri, content);
            throw new GatewayCommunicationException($"Conflict: {content}");
        }

        // Internal server error
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            Logger?.LogError("{Operation} {Url} returned 500 Internal Server Error: {Content}", operation, response.RequestMessage?.RequestUri, content);
            throw new GatewayInternalServerException($"Internal server error: {content}");
        }

        // Default error handling
        Logger?.LogError("{Operation} {Url} returned unexpected status code {StatusCode}: {Content}", 
            operation, response.RequestMessage?.RequestUri, response.StatusCode, content);
        throw new GatewayInternalServerException($"HTTP {response.StatusCode}: {content}");
    }

    /// <summary>
    /// Creates appropriate not found exception based on entity type
    /// </summary>
    protected abstract Exception CreateNotFoundException(string entityName, int? entityId);

    /// <summary>
    /// Gets entity name from type
    /// </summary>
    protected string GetEntityName<T>()
    {
        var typeName = typeof(T).Name;
        return typeName.Replace("DTO", "").Replace("Response", "");
    }

    /// <summary>
    /// Serializes object to JSON with proper configuration
    /// </summary>
    protected StringContent SerializeToJson(object obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Handles GET request with error handling and logging
    /// </summary>
    protected async Task<T> GetAsync<T>(string endpoint)
    {
        var url = $"{BaseUrl}{endpoint}";
        Logger?.LogInformation("Sending GET request to {Url}", url);

        try
        {
            var response = await HttpClient.GetAsync(url).ConfigureAwait(false);
            return await HandleResponseAsync<T>(response, "GET").ConfigureAwait(false);
        }
        catch (TaskCanceledException ex)
        {
            Logger?.LogError(ex, "GET request to {Url} timed out", url);
            throw new GatewayTimeoutException($"Request to {url} timed out");
        }
        catch (HttpRequestException ex)
        {
            Logger?.LogError(ex, "GET request to {Url} failed: {Message}", url, ex.Message);
            throw new GatewayCommunicationException($"HTTP request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles POST request with error handling and logging
    /// </summary>
    protected async Task<T> PostAsync<T>(string endpoint, object data)
    {
        var url = $"{BaseUrl}{endpoint}";
        Logger?.LogInformation("Sending POST request to {Url}", url);

        try
        {
            var content = SerializeToJson(data);
            var response = await HttpClient.PostAsync(url, content).ConfigureAwait(false);
            return await HandleResponseAsync<T>(response, "POST").ConfigureAwait(false);
        }
        catch (TaskCanceledException ex)
        {
            Logger?.LogError(ex, "POST request to {Url} timed out", url);
            throw new GatewayTimeoutException($"Request to {url} timed out");
        }
        catch (HttpRequestException ex)
        {
            Logger?.LogError(ex, "POST request to {Url} failed: {Message}", url, ex.Message);
            throw new GatewayCommunicationException($"HTTP request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles PUT request with error handling and logging
    /// </summary>
    protected async Task<T> PutAsync<T>(string endpoint, object data)
    {
        var url = $"{BaseUrl}{endpoint}";
        Logger?.LogInformation("Sending PUT request to {Url}", url);

        try
        {
            var content = SerializeToJson(data);
            var response = await HttpClient.PutAsync(url, content).ConfigureAwait(false);
            return await HandleResponseAsync<T>(response, "PUT").ConfigureAwait(false);
        }
        catch (TaskCanceledException ex)
        {
            Logger?.LogError(ex, "PUT request to {Url} timed out", url);
            throw new GatewayTimeoutException($"Request to {url} timed out");
        }
        catch (HttpRequestException ex)
        {
            Logger?.LogError(ex, "PUT request to {Url} failed: {Message}", url, ex.Message);
            throw new GatewayCommunicationException($"HTTP request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles PATCH request with error handling and logging
    /// </summary>
    protected async Task<T> PatchAsync<T>(string endpoint, object data)
    {
        var url = $"{BaseUrl}{endpoint}";
        Logger?.LogInformation("Sending PATCH request to {Url}", url);

        try
        {
            var content = SerializeToJson(data);
            var patchContent = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            var response = await HttpClient.PatchAsync(url, patchContent).ConfigureAwait(false);
            return await HandleResponseAsync<T>(response, "PATCH").ConfigureAwait(false);
        }
        catch (TaskCanceledException ex)
        {
            Logger?.LogError(ex, "PATCH request to {Url} timed out", url);
            throw new GatewayTimeoutException($"Request to {url} timed out");
        }
        catch (HttpRequestException ex)
        {
            Logger?.LogError(ex, "PATCH request to {Url} failed: {Message}", url, ex.Message);
            throw new GatewayCommunicationException($"HTTP request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles DELETE request with error handling and logging
    /// </summary>
    protected async Task<bool> DeleteAsync(string endpoint)
    {
        var url = $"{BaseUrl}{endpoint}";
        Logger?.LogInformation("Sending DELETE request to {Url}", url);

        try
        {
            var response = await HttpClient.DeleteAsync(url).ConfigureAwait(false);
            return await HandleResponseAsync<bool>(response, "DELETE").ConfigureAwait(false);
        }
        catch (TaskCanceledException ex)
        {
            Logger?.LogError(ex, "DELETE request to {Url} timed out", url);
            throw new GatewayTimeoutException($"Request to {url} timed out");
        }
        catch (HttpRequestException ex)
        {
            Logger?.LogError(ex, "DELETE request to {Url} failed: {Message}", url, ex.Message);
            throw new GatewayCommunicationException($"HTTP request failed: {ex.Message}");
        }
    }
}
