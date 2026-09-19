using MaintenancePortal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenancePortal.Infrastructure.Data.Configurations;

public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
    {
        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(4000);

        // Decision: enums stored as strings, not ints. Slightly larger on
        // disk, but the data is self-describing when you look at the table
        // directly, and reordering enum members later can't silently corrupt
        // existing rows the way integer storage can. Easy to switch to
        // HasConversion<int>() later if this ever needs to change.
        builder.Property(r => r.Category).HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.Priority).HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(r => r.TenantAssignmentId);
        builder.HasIndex(r => r.Status);

        builder.HasMany(r => r.Photos)
            .WithOne(p => p.Request)
            .HasForeignKey(p => p.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Comments)
            .WithOne(c => c.Request)
            .HasForeignKey(c => c.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
