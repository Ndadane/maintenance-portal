using MaintenancePortal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenancePortal.Infrastructure.Data.Configurations;

public class TenantAssignmentConfiguration : IEntityTypeConfiguration<TenantAssignment>
{
    public void Configure(EntityTypeBuilder<TenantAssignment> builder)
    {
        // "GET /api/my-unit" and tenant-scoped request queries both filter
        // on TenantId, then check active status from the dates.
        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.UnitId);

        // Overlap prevention is application-level only for the MVP (see
        // TenantAssignment.OverlapsWith). A database-level exclusion
        // constraint (btree_gist) is a documented future enhancement, not
        // implemented here yet.

        builder.HasMany(a => a.MaintenanceRequests)
            .WithOne(r => r.TenantAssignment)
            .HasForeignKey(r => r.TenantAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
