using MaintenancePortal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenancePortal.Infrastructure.Data.Configurations;

public class RequestPhotoConfiguration : IEntityTypeConfiguration<RequestPhoto>
{
    public void Configure(EntityTypeBuilder<RequestPhoto> builder)
    {
        builder.Property(p => p.StorageKey).IsRequired().HasMaxLength(1000);
    }
}
