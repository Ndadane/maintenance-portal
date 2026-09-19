namespace MaintenancePortal.Core.Entities;

/// <summary>
/// A single message in a maintenance request's conversation thread, replacing
/// fragmented SMS/text communication between tenant and landlord.
/// AuthorId is a Guid FK to the Identity user (no navigation property, same
/// reasoning as Property.LandlordId - Core does not reference ApplicationUser).
/// </summary>
public class RequestComment
{
    public Guid Id { get; set; }

    public Guid RequestId { get; set; }
    public MaintenanceRequest? Request { get; set; }

    public Guid AuthorId { get; set; }

    public string Body { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
