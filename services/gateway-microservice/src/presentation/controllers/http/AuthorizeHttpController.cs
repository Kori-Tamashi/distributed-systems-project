using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using core.configuration;
using presentation.dto.http;

namespace presentation.controllers.http;

/// <summary>
/// OAuth2 authorization endpoint for obtaining JWT tokens from Keycloak
/// </summary>
[ApiController]
[Route("api/v1")]
[AllowAnonymous]
public class AuthorizeHttpController : ControllerBase
{
    private readonly OidcSettings _oidcSettings;
    private readonly ILogger<AuthorizeHttpController> _logger;

    public AuthorizeHttpController(OidcSettings oidcSettings, ILogger<AuthorizeHttpController> logger)
    {
        _oidcSettings = oidcSettings;
        _logger = logger;
    }

    /// <summary>
    /// Exchange username/password for JWT token (ROPC flow)
    /// </summary>
    [HttpPost("authorize")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<TokenResponse>> Authorize([FromBody] TokenRequest request)
    {
        try
        {
            _logger.LogInformation("Token request for user: {Username}", request.Username);

            using var httpClient = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", _oidcSettings.ClientId),
                new KeyValuePair<string, string>("client_secret", _oidcSettings.ClientSecret),
                new KeyValuePair<string, string>("username", request.Username),
                new KeyValuePair<string, string>("password", request.Password),
                new KeyValuePair<string, string>("scope", "openid profile email")
            });

            var response = await httpClient.PostAsync(_oidcSettings.TokenEndpoint, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Keycloak returned error: {StatusCode}", response.StatusCode);
                var error = await response.Content.ReadAsStringAsync();
                // Check if it's authentication error (invalid credentials) vs infrastructure error
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && error.Contains("invalid_grant"))
                {
                    return Unauthorized(new ErrorResponse("Invalid credentials"));
                }
                // Infrastructure error (Keycloak unreachable or other error)
                return StatusCode(502, new ErrorResponse("Keycloak unavailable"));
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            
            if (tokenResponse == null)
            {
                return StatusCode(502, new ErrorResponse("Invalid token response from Keycloak"));
            }

            _logger.LogInformation("Token issued for user: {Username}", request.Username);
            return Ok(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token exchange");
            return StatusCode(502, new ErrorResponse("Token exchange failed: " + ex.Message));
        }
    }

    /// <summary>
    /// Callback endpoint (placeholder for authorization code flow)
    /// </summary>
    [HttpGet("callback")]
    public IActionResult Callback([FromQuery] string code, [FromQuery] string state)
    {
        // Placeholder for authorization code flow
        // Currently only ROPC flow is implemented
        return Ok(new { message = "Callback endpoint - ROPC flow only" });
    }
}

/// <summary>
/// Token request model (ROPC flow)
/// </summary>
public class TokenRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

/// <summary>
/// Token response model
/// </summary>
public class TokenResponse
{
    public string access_token { get; set; } = "";
    public string token_type { get; set; } = "Bearer";
    public int expires_in { get; set; }
    public string refresh_token { get; set; } = "";
    public string scope { get; set; } = "";
}
