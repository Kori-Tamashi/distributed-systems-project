using System.Net.Http.Headers;

namespace presentation.http;

/// <summary>
/// DelegatingHandler used only in test mode: rewrites Authorization header on
/// outgoing requests to a pre-generated test JWT (HS256).
/// Needed because ForwardAuthHandler overwrites Authorization from HttpContext.User,
/// which is empty or invalid when the request originates from a Newman run against
/// a Gateway container in test mode.
/// </summary>
public class TestAuthHandler : DelegatingHandler
{
    private readonly string? _token;

    public TestAuthHandler(string? token)
    {
        _token = token;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
