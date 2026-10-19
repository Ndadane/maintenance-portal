# Maintenance Portal

A maintenance-request tracker for small independent landlords (2–15 units), built to replace the usual workflow of text messages and scattered photos with a system that has real history, real status tracking, and correct data privacy between tenants.

This is a personal/portfolio project. It is not deployed for public use.

## The problem

Small landlords don't run software built for their scale — most property management tools target companies with hundreds of units. In practice, maintenance gets tracked over text message: no history, no photo trail, no way to see what's open versus resolved, and no reliable way to keep one tenant's complaints private from the next tenant who moves into the same unit.

That last point turned out to be the interesting engineering problem in this project, described below.

## What it does

- Landlords manage properties, units, and tenant assignments, and see all maintenance requests across their portfolio with status and priority controls.
- Tenants see their current unit, submit maintenance requests, and track them through to resolution.
- Requests support photo attachments and a comment thread between tenant and landlord.
- Status changes trigger an email notification to the tenant.

## The hard problem: access tied to occupancy, not to the unit

A maintenance request shouldn't be visible just because someone currently lives in a unit. If Alice lived in Unit 2B from January to June and filed a request, and Bob moves in from July, Bob should not be able to see Alice's request — but the landlord still needs the full history.

The fix was modeling occupancy as its own entity, `TenantAssignment`, with a `MoveInDate` and an optional `MoveOutDate`. A `MaintenanceRequest` belongs to the assignment it was created under, not directly to a tenant or a unit:

```
MaintenanceRequest → TenantAssignment → Unit → Property → Landlord
```

A tenant's access to a request requires two things to both be true: the assignment is theirs, *and* that assignment is currently active (`MoveInDate <= today AND (MoveOutDate IS NULL OR MoveOutDate > today)`). There's no stored `IsActive` flag — active status is always calculated from the dates, so it can't drift out of sync with reality. A landlord's access has no date check at all; it's permanent for anything under their own properties, since retaining history is exactly what they're supposed to be able to do.

This was manually tested end-to-end with real accounts: after Alice moved out and Bob moved in, Bob's request for Alice's old maintenance ticket returned `404`, and the landlord's identical request returned the full record.

## Authorization, more generally

Two different strategies are used depending on the shape of the endpoint:

- **Query-level**, for list endpoints. The authorization is the `WHERE` clause — e.g. `GET /api/properties` only ever queries rows where `LandlordId == currentUser.Id`. There's no way to leak another landlord's data because the database is never asked for it.
- **Resource-based**, for single-item endpoints. The resource is loaded, then explicitly checked against the caller (`GET /api/requests/{id}`).

Unauthorized access to a specific resource returns `404`, not `403`. A `403` confirms the resource exists and just isn't yours, which leaks information to anyone probing IDs. A `404` gives nothing away.

## Architecture

Modular monolith, not microservices — one deployable backend with enforced internal boundaries, since a project this size doesn't have the organizational-scaling problem microservices are meant to solve.

```
MaintenancePortal.Api             HTTP concerns: controllers, DTOs, auth wiring
MaintenancePortal.Core            Pure domain: entities, enums, business rules — zero package references
MaintenancePortal.Infrastructure  EF Core, Identity, JWT, object storage, email
```

Core doesn't know Postgres, ASP.NET Core, or EF Core exist. Dependencies only point one way: `Api → Infrastructure → Core`.

## Tech stack

**Backend:** ASP.NET Core Web API (.NET 10), C#, EF Core, PostgreSQL (hosted on [Neon](https://neon.tech)), ASP.NET Core Identity + JWT

**Frontend:** React + TypeScript (Vite), React Router, Tailwind CSS

**File storage:** [Backblaze B2](https://www.backblaze.com/) (S3-compatible), accessed via presigned URLs — photo bytes go directly from the browser to storage and never pass through the API server. The API only ever handles the object key and metadata.

**Email:** SMTP via [Brevo](https://www.brevo.com/), sent from an ASP.NET Core background hosted service so status-change notifications don't block the HTTP response.

## Notable implementation decisions

- **Enums stored as strings** in Postgres, not integers, so the data stays readable and isn't at risk of silent corruption if enum values get reordered later.
- **Tenant accounts are auto-provisioned.** When a landlord assigns a tenant by email and no account exists yet, one is created with a random temporary password, returned once for the landlord to relay. There's no invite-email flow yet — a deliberate, acknowledged gap rather than a blocker.
- **Assignment overlap is checked in application code**, not at the database level: two tenants can't be assigned to the same unit with overlapping date ranges. This only holds if every write goes through the same code path — a database-level exclusion constraint would close the remaining race condition, and is a known, documented gap rather than an oversight.
- **DTOs everywhere**, in both directions. Entities are never serialized directly — this avoids leaking internal fields, avoids circular-reference serialization crashes from EF navigation properties, and lets the API contract evolve independently of the database schema.
- **PATCH semantics on status updates**: both `Status` and `Priority` are optional on the request body, and only the fields actually provided are applied.
- **Photo validation happens after upload, not before.** A presigned URL can constrain content type, but not file size reliably. The API checks the real uploaded object's size and type against the bucket after the fact, and deletes anything that doesn't qualify.
- **Retry-on-failure is enabled for EF Core's database calls**, added after hitting real transient connection drops against Neon over the network — not a fix for a bug, a tolerance for occasional network blips.
- **Secrets never touch the repo.** Local development uses `dotnet user-secrets`; nothing sensitive lives in `appsettings.json`.

## Known limitations

- No automated test suite yet. Authorization behavior was verified manually against real accounts and real API calls (including the moved-out-tenant scenario above), not with an automated regression suite.
- The email background service is in-memory — queued notifications are lost if the process restarts before they're sent. Fine for this scale; a production version would use a durable queue.
- The frontend stores its JWT in `localStorage`, which is simple but vulnerable to XSS in a way an httpOnly cookie wouldn't be. Trading off implementation time against that risk was a deliberate call for this project's scope.
- No automated CI/CD; deployment is manual.

## Project structure

```
MaintenancePortal/
├── src/
│   ├── MaintenancePortal.Api/
│   ├── MaintenancePortal.Core/
│   └── MaintenancePortal.Infrastructure/
├── tests/
│   └── MaintenancePortal.Tests/
└── frontend/
```

## Running locally

Requires the .NET SDK, Node.js, and a PostgreSQL connection string (Neon or otherwise).

```bash
# Backend
dotnet user-secrets set "ConnectionStrings:Default" "<your-connection-string>" --project src/MaintenancePortal.Api
dotnet user-secrets set "Jwt:SigningKey" "<a-random-string>" --project src/MaintenancePortal.Api
dotnet ef database update --project src/MaintenancePortal.Infrastructure --startup-project src/MaintenancePortal.Api
dotnet run --project src/MaintenancePortal.Api

# Frontend
cd frontend
npm install
npm run dev
```

Photo upload and email notifications require Backblaze B2 and Brevo SMTP credentials, also set via `dotnet user-secrets` (`Storage:*` and `Email:*`).
