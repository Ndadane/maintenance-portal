using System.ComponentModel.DataAnnotations;

namespace MaintenancePortal.Api.Contracts.TenantAssignments;

public class AssignTenantRequest
{
	[Required, EmailAddress]
	public string TenantEmail { get; set; } = string.Empty;

	[Required]
	public DateOnly MoveInDate { get; set; }

	/// <summary>Leave null for an open-ended (current) occupancy.</summary>
	public DateOnly? MoveOutDate { get; set; }
}

public class TenantAssignmentResponse
{
	public Guid Id { get; set; }
	public Guid UnitId { get; set; }
	public Guid TenantId { get; set; }
	public string TenantEmail { get; set; } = string.Empty;
	public DateOnly MoveInDate { get; set; }
	public DateOnly? MoveOutDate { get; set; }

	/// <summary>True if no account existed for this email and one was created automatically.</summary>
	public bool TenantWasAutoCreated { get; set; }

	/// <summary>
	/// Only populated when TenantWasAutoCreated is true. There's no email
	/// invite flow yet (that's Phase 6) - the landlord needs to pass this to
	/// the tenant directly for now so they can log in and change it.
	/// </summary>
	public string? TemporaryPassword { get; set; }
}

public class EndAssignmentRequest
{
	[Required]
	public DateOnly MoveOutDate { get; set; }
}
