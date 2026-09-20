using System.Security.Claims;

namespace MaintenancePortal.Api.Common;

/// <summary>
/// Pulls the authenticated user's id out of their JWT claims. Checks both
/// "sub" and ClaimTypes.NameIdentifier because ASP.NET Core's JWT handler
/// sometimes remaps "sub" to the latter depending on configuration - this
/// works regardless of which one ends up populated.
/// </summary>
public static class ClaimsPrincipalExtensions
{
	public static Guid GetUserId(this ClaimsPrincipal user)
	{
		var value = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

		if (value is null || !Guid.TryParse(value, out var id))
		{
			throw new InvalidOperationException("Authenticated user is missing a valid id claim.");
		}

		return id;
	}
}