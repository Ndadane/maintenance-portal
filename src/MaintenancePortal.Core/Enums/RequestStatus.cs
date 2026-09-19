namespace MaintenancePortal.Core.Enums;

/// <summary>
/// Fixed workflow vocabulary for a MaintenanceRequest.
/// Submitted -> Acknowledged -> InProgress -> Resolved -> Closed
/// Stored as a string in the database (see AppDbContext configuration) so the
/// data stays human-readable in the DB and enum values can be safely reordered.
/// </summary>
public enum RequestStatus
{
    Submitted = 0,
    Acknowledged = 1,
    InProgress = 2,
    Resolved = 3,
    Closed = 4
}
