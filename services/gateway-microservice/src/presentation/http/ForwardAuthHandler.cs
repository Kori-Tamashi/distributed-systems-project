using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace presentation.http;

/// <summary>
/// Forwards the incoming user's Authorization header to outgoing downstream requests.
/// Falls back to a service-account token (client_credentials) when no HttpContext is available
/// (e.g. background retry-queue jobs).
/// </summary>
public class ForwardAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;
    private readonly ServiceTokenProvider _serviceTokenProvider;

    public ForwardAuthHandler(
        IHttpContextAccessor accessor,
        ServiceTokenProvider serviceTokenProvider)
    {
        _accessor = accessor;
        _serviceTokenProvider = serviceTokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? token = null;

        // Try to get user token from HttpContext first
        var httpContext = _accessor.HttpContext;
        if (httpContext != null)
        {
            var header = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(header) && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = header.Substring("Bearer ".Length).Trim();
            }
        }

        // Fallback to service account token if no user context (e.g. retry-queue)
        if (string.IsNullOrEmpty(token))
        {
            token = await _serviceTokenProvider.GetTokenAsync(cancellationToken);
        }

        // Add Authorization header if we have a token
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
