using MaintenancePortal.Core.Enums;

namespace MaintenancePortal.Core.Entities;

/// <summary>
/// A maintenance problem reported by a tenant. Deliberately owned by a
/// TenantAssignment (not TenantId + UnitId directly) - see TenantAssignment
/// for why. This gives a single authoritative chain:
/// MaintenanceRequest -> TenantAssignment -> Unit -> Property -> Landlord.
/// </summary>
public class MaintenanceRequest
{
    public Guid Id { get; set; }

    public Guid TenantAssignmentId { get; set; }
    public TenantAssignment? TenantAssignment { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public RequestCategory Category { get; set; }

    public RequestPriority Priority { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Submitted;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public ICollection<RequestPhoto> Photos { get; set; } = new List<RequestPhoto>();

    public ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();
}
