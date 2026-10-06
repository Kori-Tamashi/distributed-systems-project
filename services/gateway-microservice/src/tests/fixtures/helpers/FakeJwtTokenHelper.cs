using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace tests.fixtures.helpers;

/// <summary>
/// Helper to generate fake JWT tokens for integration tests
/// </summary>
public static class FakeJwtTokenHelper
{
    public static string GenerateFakeToken(string username = "testuser")
    {
        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("ThisIsATestSecretKeyForIntegrationTests1234567890"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim("preferred_username", username),
            new Claim(JwtRegisteredClaimNames.Email, username + "@test.com"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("scope", "openid profile email")
        };

        var token = new JwtSecurityToken(
            issuer: "http://localhost:8888/realms/rsoi",
            audience: "gateway",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
