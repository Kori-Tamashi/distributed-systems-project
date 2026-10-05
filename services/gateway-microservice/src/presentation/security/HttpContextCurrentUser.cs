using System.Security.Claims;
using core.security;
using Microsoft.AspNetCore.Http;

namespace presentation.security;

/// <summary>
/// Implementation of ICurrentUser that reads from HttpContext via IHttpContextAccessor
/// </summary>
public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string? UserId => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? _accessor.HttpContext?.User?.FindFirst("sub")?.Value;

    public string? Username => _accessor.HttpContext?.User?.FindFirst("preferred_username")?.Value
                            ?? _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? DisplayName => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
                               ?? _accessor.HttpContext?.User?.FindFirst("name")?.Value;

    public string? FirstName => _accessor.HttpContext?.User?.FindFirst("given_name")?.Value;

    public string? LastName => _accessor.HttpContext?.User?.FindFirst("family_name")?.Value;

    public string? Email => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                         ?? _accessor.HttpContext?.User?.FindFirst("email")?.Value;

    public bool EmailVerified => _accessor.HttpContext?.User?.HasClaim("email_verified", "true") ?? false;

    public IEnumerable<string> Scopes
    {
        get
        {
            var scopeClaim = _accessor.HttpContext?.User?.FindFirst("scope")?.Value;
            if (string.IsNullOrEmpty(scopeClaim))
                return Enumerable.Empty<string>();

            return scopeClaim.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public bool IsAuthenticated => _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
