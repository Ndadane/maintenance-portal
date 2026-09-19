namespace MaintenancePortal.Core.Entities;

/// <summary>
/// A unit belongs to exactly one property (e.g. "Unit 2B" inside
/// "15 Main Street"). Authorization chain: Landlord -> Property -> Unit.
/// </summary>
public class Unit
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }
    public Property? Property { get; set; }

    public string UnitLabel { get; set; } = string.Empty;

    public ICollection<TenantAssignment> TenantAssignments { get; set; } = new List<TenantAssignment>();
}
