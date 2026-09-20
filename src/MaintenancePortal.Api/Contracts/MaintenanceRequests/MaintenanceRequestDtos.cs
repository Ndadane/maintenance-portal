using System.ComponentModel.DataAnnotations;
using MaintenancePortal.Core.Enums;

namespace MaintenancePortal.Api.Contracts.MaintenanceRequests;

public class MyUnitResponse
{
	public Guid AssignmentId { get; set; }
	public Guid UnitId { get; set; }
	public string UnitLabel { get; set; } = string.Empty;
	public Guid PropertyId { get; set; }
	public string PropertyName { get; set; } = string.Empty;
	public string PropertyAddress { get; set; } = string.Empty;
	public DateOnly MoveInDate { get; set; }
}

public class CreateMaintenanceRequestRequest
{
	[Required, MaxLength(200)]
	public string Title { get; set; } = string.Empty;

	[Required, MaxLength(4000)]
	public string Description { get; set; } = string.Empty;

	[Required]
	public RequestCategory Category { get; set; }

	[Required]
	public RequestPriority Priority { get; set; }
}

/// <summary>
/// PATCH semantics: both fields optional, only the ones provided get
/// changed. Landlord-only. Status is the primary purpose of this endpoint;
/// Priority is included per the documented business rule that landlords
/// may adjust priority after the tenant's initial submission.
/// </summary>
public class UpdateMaintenanceRequestRequest
{
	public RequestStatus? Status { get; set; }
	public RequestPriority? Priority { get; set; }
}

public class MaintenanceRequestResponse
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public RequestCategory Category { get; set; }
	public RequestPriority Priority { get; set; }
	public RequestStatus Status { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? ResolvedAt { get; set; }

	// Denormalized context so callers don't need a second round trip.
	public Guid UnitId { get; set; }
	public string UnitLabel { get; set; } = string.Empty;
	public Guid PropertyId { get; set; }
	public string PropertyName { get; set; } = string.Empty;
}
