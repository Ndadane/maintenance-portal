namespace MaintenancePortal.Infrastructure.Auth;

/// <summary>
/// Bound from the "Jwt" section of appsettings.json / user secrets via the
/// Options pattern (builder.Services.Configure&lt;JwtSettings&gt;(...)).
/// </summary>
public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Symmetric signing key. Must be kept out of source control in real
    /// deployments (user secrets locally, environment variable / secret
    /// manager in production) - the placeholder in appsettings.json is a
    /// dev-only value.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;
}
