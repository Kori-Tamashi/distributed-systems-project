namespace core.security;

/// <summary>
/// Represents the current authenticated user from JWT token
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// User ID (subject) from JWT
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Username (preferred_username) from JWT
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Full name from JWT
    /// </summary>
    string? DisplayName { get; }

    /// <summary>
    /// First name from JWT
    /// </summary>
    string? FirstName { get; }

    /// <summary>
    /// Last name from JWT
    /// </summary>
    string? LastName { get; }

    /// <summary>
    /// Email from JWT
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Whether email is verified
    /// </summary>
    bool EmailVerified { get; }

    /// <summary>
    /// Scopes granted in the token
    /// </summary>
    IEnumerable<string> Scopes { get; }

    /// <summary>
    /// Whether the user is authenticated
    /// </summary>
    bool IsAuthenticated => UserId != null;
}
