using System.Net.Http.Json;
using System.Text.Json.Serialization;
using core.configuration;

namespace presentation.http;

/// <summary>
/// Fetches a service-account token from Keycloak via client_credentials grant.
/// Used by ForwardAuthHandler as a fallback when no user HttpContext is available
/// (e.g. background retry-queue jobs).
/// </summary>
public class ServiceTokenProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OidcSettings _oidc;
    private readonly ILogger<ServiceTokenProvider> _logger;

    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public ServiceTokenProvider(
        IHttpClientFactory httpClientFactory,
        OidcSettings oidc,
        ILogger<ServiceTokenProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _oidc = oidc;
        _logger = logger;
    }

    public async Task<string?> GetTokenAsync(CancellationToken ct = default)
    {
        // Return cached token if still valid (with 30s buffer)
        if (_cachedToken != null && _expiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
            return _cachedToken;

        await _lock.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock
            if (_cachedToken != null && _expiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
                return _cachedToken;

            var tokenEndpoint = _oidc.TokenEndpoint;

            using var client = _httpClientFactory.CreateClient("token");
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _oidc.ClientId,
                ["client_secret"] = _oidc.ClientSecret,
            });

            using var resp = await client.PostAsync(tokenEndpoint, content, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Service token fetch failed: {Status} - {Content}", resp.StatusCode, await resp.Content.ReadAsStringAsync(ct));
                return null;
            }

            var json = await resp.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
            if (json?.AccessToken is null)
            {
                _logger.LogWarning("Service token response has no access_token");
                return null;
            }

            _cachedToken = json.AccessToken;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(json.ExpiresIn > 0 ? json.ExpiresIn : 60);
            _logger.LogInformation("Service token fetched, ttl={Ttl}s", json.ExpiresIn);
            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
    }
}
