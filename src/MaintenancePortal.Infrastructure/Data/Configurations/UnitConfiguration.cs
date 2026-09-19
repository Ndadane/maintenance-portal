using MaintenancePortal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenancePortal.Infrastructure.Data.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.Property(u => u.UnitLabel).IsRequired().HasMaxLength(100);

        builder.HasMany(u => u.TenantAssignments)
            .WithOne(a => a.Unit)
            .HasForeignKey(a => a.UnitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
