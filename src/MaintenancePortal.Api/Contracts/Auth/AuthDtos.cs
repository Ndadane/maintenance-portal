using System.ComponentModel.DataAnnotations;

namespace MaintenancePortal.Api.Contracts.Auth;

/// <summary>
/// MVP simplification: the user picks their own role at registration. A
/// production app would likely have landlords invite tenants rather than
/// letting anyone self-register as either role - flagged as a known
/// simplification, not something to silently harden without discussion.
/// </summary>
public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Must be exactly "Landlord" or "Tenant" - validated in the controller.</summary>
    [Required]
    public string Role { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = [];
}
