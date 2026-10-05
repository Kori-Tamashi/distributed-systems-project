namespace core.configuration;

/// <summary>
/// OpenID Connect / OAuth2 settings for Keycloak integration
/// </summary>
public class OidcSettings
{
    /// <summary>
    /// Keycloak realm URL (e.g., http://localhost:8888/realms/rsoi)
    /// </summary>
    public string Issuer { get; set; } = "http://localhost:8888/realms/rsoi";

    /// <summary>
    /// JWKS URI for token validation (e.g., http://localhost:8888/realms/rsoi/protocol/openid-connect/certs)
    /// </summary>
    public string JwksUri { get; set; } = "http://localhost:8888/realms/rsoi/protocol/openid-connect/certs";

    /// <summary>
    /// Token endpoint URL (e.g., http://localhost:8888/realms/rsoi/protocol/openid-connect/token)
    /// </summary>
    public string TokenEndpoint { get; set; } = "http://localhost:8888/realms/rsoi/protocol/openid-connect/token";

    /// <summary>
    /// Client ID for ROPC flow
    /// </summary>
    public string ClientId { get; set; } = "gateway";

    /// <summary>
    /// Client Secret for ROPC flow
    /// </summary>
    public string ClientSecret { get; set; } = "";

    /// <summary>
    /// Expected issuer in JWT
    /// </summary>
    public string ValidIssuer { get; set; } = "http://localhost:8888/realms/rsoi";

    /// <summary>
    /// Required scope for token validation
    /// </summary>
    public string RequiredScope { get; set; } = "openid";
}
