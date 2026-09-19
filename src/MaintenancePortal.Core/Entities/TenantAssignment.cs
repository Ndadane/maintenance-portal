namespace MaintenancePortal.Core.Entities;

/// <summary>
/// Records a tenant's occupancy of a unit over a time range. This is the
/// authorization boundary for maintenance requests: a MaintenanceRequest
/// belongs to a TenantAssignment (not directly to Tenant + Unit), so that
/// when a tenant moves out and a new tenant moves in, the new tenant does
/// not inherit visibility into the previous tenant's request history.
///
/// IsActive is intentionally NOT a stored column. Storing a redundant
/// boolean alongside MoveInDate/MoveOutDate risks the two disagreeing
/// (e.g. MoveOutDate in the past but IsActive still true). Active status is
/// always derived from the dates - see IsActiveAsOf.
/// </summary>
public class TenantAssignment
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public DateOnly MoveInDate { get; set; }

    /// <summary>Null means "still occupying as of today."</summary>
    public DateOnly? MoveOutDate { get; set; }

    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();

    /// <summary>
    /// Derives whether this assignment is active on a given date.
    /// MoveInDate &lt;= asOf AND (MoveOutDate is null OR MoveOutDate &gt; asOf).
    /// </summary>
    public bool IsActiveAsOf(DateOnly asOf)
    {
        return MoveInDate <= asOf && (MoveOutDate is null || MoveOutDate > asOf);
    }

    /// <summary>
    /// True if this assignment's date range overlaps another range for the
    /// same unit. Used by the application-level validation when creating a
    /// new assignment, to reject overlapping occupancies on the same unit.
    /// </summary>
    public bool OverlapsWith(DateOnly otherMoveIn, DateOnly? otherMoveOut)
    {
        var thisEnd = MoveOutDate ?? DateOnly.MaxValue;
        var otherEnd = otherMoveOut ?? DateOnly.MaxValue;

        return MoveInDate < otherEnd && otherMoveIn < thisEnd;
    }
}
