namespace MaintenancePortal.Core.Entities;

/// <summary>
/// A property belongs to exactly one landlord. LandlordId is a Guid FK to the
/// Identity user rather than a navigation property to ApplicationUser -
/// ApplicationUser lives in the Infrastructure layer (it depends on
/// Microsoft.AspNetCore.Identity), and Core must not depend on Infrastructure.
/// </summary>
public class Property
{
    public Guid Id { get; set; }

    public Guid LandlordId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}
