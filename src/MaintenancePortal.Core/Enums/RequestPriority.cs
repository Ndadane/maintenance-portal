namespace MaintenancePortal.Core.Enums;

/// <summary>
/// Tenant sets this on creation; landlord may change it afterward.
/// </summary>
public enum RequestPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}
