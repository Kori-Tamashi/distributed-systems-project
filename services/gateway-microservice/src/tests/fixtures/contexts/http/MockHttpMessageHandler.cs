using System.Net;
using System.Text;
using System.Text.Json;

namespace tests.fixtures.contexts.http;

/// <summary>
/// Custom mock HTTP message handler for testing HTTP gateways
/// Allows configuration of response behavior without mocking protected methods
/// </summary>
public class MockHttpMessageHandler : DelegatingHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

    /// <summary>
    /// Creates a mock handler with custom response logic
    /// </summary>
    public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    /// <summary>
    /// Creates a mock handler that returns a specific response
    /// </summary>
    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _handler = async (req, ct) => await Task.FromResult(response);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        return _handler(request, cancellationToken);
    }
}

/// <summary>
/// Factory for creating mock HTTP responses
/// </summary>
public static class MockHttpResponses
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Creates a success response with serialized data
    /// </summary>
    public static HttpResponseMessage Success<T>(T data, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(data, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };
    }

    /// <summary>
    /// Creates a 404 Not Found response
    /// </summary>
    public static HttpResponseMessage NotFound()
    {
        return new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("", Encoding.UTF8, "application/json")
        };
    }

    /// <summary>
    /// Creates a 204 No Content response
    /// </summary>
    public static HttpResponseMessage NoContent()
    {
        return new HttpResponseMessage(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Creates a 500 Internal Server Error response
    /// </summary>
    public static HttpResponseMessage InternalServerError(string message = "Internal server error")
    {
        return new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(message, Encoding.UTF8, "application/json")
        };
    }
}
