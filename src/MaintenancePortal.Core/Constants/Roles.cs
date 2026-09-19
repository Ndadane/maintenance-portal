namespace MaintenancePortal.Core.Constants;

/// <summary>
/// Canonical role names used with ASP.NET Core Identity's role system
/// (IdentityRole, UserManager.AddToRoleAsync, [Authorize(Roles = ...)]).
/// Using constants instead of magic strings scattered across the codebase.
/// </summary>
public static class Roles
{
    public const string Landlord = "Landlord";
    public const string Tenant = "Tenant";
}
