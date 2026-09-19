# Maintenance Portal — Backend Scaffold (Phase 1)

Lightweight maintenance-tracking portal for small independent landlords (2–15 units).
This is the Phase 1 scaffold: solution structure, domain entities, EF Core + PostgreSQL,
ASP.NET Core Identity + JWT auth, and Landlord/Tenant roles. No business endpoints
(Property/Unit/MaintenanceRequest CRUD, authorization handlers) yet — that's Phase 2+.

## Structure

```
MaintenancePortal.sln
src/
  MaintenancePortal.Api/             HTTP entry point: Program.cs, controllers, DTOs, auth config
  MaintenancePortal.Core/            Pure domain: entities, enums, ITokenService abstraction
  MaintenancePortal.Infrastructure/  EF Core, PostgreSQL, Identity's ApplicationUser, JWT generation
tests/
  MaintenancePortal.Tests/           Empty for now — filled in from Phase 3 onward
```

**Dependency direction:** Api → Infrastructure → Core, and Api → Core. Core has zero
package references — it doesn't know PostgreSQL, EF Core, or ASP.NET Core exist.

## What's implemented

- **Domain entities** (Core): `Property`, `Unit`, `TenantAssignment`, `MaintenanceRequest`,
  `RequestPhoto`, `RequestComment` — matching the finalized domain model, including
  `TenantAssignment.IsActiveAsOf()` (derived active status, no redundant `IsActive` column)
  and `OverlapsWith()` (used for the overlap-prevention business rule later).
- **Identity**: `ApplicationUser : IdentityUser<Guid>` in Infrastructure, `Landlord`/`Tenant`
  roles seeded on startup.
- **JWT auth**: `POST /api/auth/register`, `POST /api/auth/login`, both returning a signed
  JWT with the user's role(s) as claims. `[Authorize(Roles = "Landlord")]` etc. will work
  against these tokens once resource endpoints exist.
- **EF Core configuration**: relationships, indexes on the columns authorization queries
  will filter on (`Property.LandlordId`, `TenantAssignment.TenantId`/`UnitId`), enums stored
  as strings.

## What's intentionally NOT here yet

Per the build order: Property/Unit/TenantAssignment CRUD endpoints, resource-based
authorization handlers, MaintenanceRequest endpoints, photo upload, comments, background
email notifications, and the React frontend. Also not decided yet: exact deployment
provider, S3/R2 provider, email provider, 403-vs-404 for unauthorized resource access,
and database-level overlap constraints — these are flagged as open decisions, not silently
resolved.

## Next step

Follow `SETUP_COMMANDS.md` to add the NuGet packages, set your Postgres connection string,
run the first migration, and confirm `/api/auth/register` + `/api/auth/login` work end to
end. Then we move to Phase 2 (Property/Unit/TenantAssignment).
