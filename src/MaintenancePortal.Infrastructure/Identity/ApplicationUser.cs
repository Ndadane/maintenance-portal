using Microsoft.AspNetCore.Identity;

namespace MaintenancePortal.Infrastructure.Identity;

/// <summary>
/// Our application's user, extending ASP.NET Core Identity's IdentityUser.
/// Identity already gives us Email, PasswordHash, security stamps, and role
/// membership - we never hand-roll password hashing. This class only adds
/// application-specific fields (currently just FullName).
///
/// Lives in Infrastructure, not Core: it depends on Microsoft.AspNetCore.Identity,
/// which is an infrastructure/framework technology. Core entities (Property,
/// MaintenanceRequest, etc.) reference users only by Guid, never by this type,
/// so Core stays free of that dependency.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
}
