using Microsoft.AspNetCore.Http;

namespace presentation.http;

/// <summary>
/// Delegating handler that forwards Authorization header from incoming request
/// to downstream services (flight, ticket, bonus microservices)
/// </summary>
public class ForwardAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    public ForwardAuthHandler(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var auth = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();
        
        if (!string.IsNullOrEmpty(auth))
        {
            request.Headers.TryAddWithoutValidation("Authorization", auth);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
