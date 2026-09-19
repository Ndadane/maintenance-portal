using MaintenancePortal.Core.Entities;
using MaintenancePortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MaintenancePortal.Infrastructure.Data;

/// <summary>
/// Combines ASP.NET Core Identity's user/role tables with our domain tables
/// in a single DbContext. This is fine for a modular monolith with one
/// database - Identity tables (AspNetUsers, AspNetRoles, etc.) just live
/// alongside our own tables (Properties, Units, MaintenanceRequests, ...).
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<TenantAssignment> TenantAssignments => Set<TenantAssignment>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<RequestPhoto> RequestPhotos => Set<RequestPhoto>();
    public DbSet<RequestComment> RequestComments => Set<RequestComment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must run first - configures all the Identity tables (AspNetUsers, etc.)
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
