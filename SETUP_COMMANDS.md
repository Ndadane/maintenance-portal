# Setup Commands

Run these from `C:\Users\Administrator\source\repos\TaskMasterPro\MaintenancePortal`
(or wherever you extract this) in PowerShell/terminal, in order. Each `dotnet add package`
resolves the correct version for your installed .NET 10 SDK automatically — no
version numbers are hardcoded in the .csproj files for that reason.

## 1. Infrastructure packages (EF Core + PostgreSQL + Identity stores)

```powershell
cd src\MaintenancePortal.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

## 2. Api packages (JWT auth, EF design-time tools, Swagger)

```powershell
cd ..\MaintenancePortal.Api
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
```

`Microsoft.EntityFrameworkCore.Design` is needed to run `dotnet ef migrations add` — the
design-time tools live in the Api project because that's the project that gets executed.

## 3. EF Core CLI tool (once, globally, if you don't already have it)

```powershell
dotnet tool install --global dotnet-ef
```

Verify with `dotnet ef --version`.

## 4. Restore and build

```powershell
cd ..\..
dotnet restore
dotnet build
```

If this fails, send me the exact error output before we go further — don't guess-fix it.

## 5. PostgreSQL connection string

Edit `src/MaintenancePortal.Api/appsettings.json` → `ConnectionStrings:Default` with your
real Postgres host/port/db/user/password. If you don't have Postgres running locally yet,
tell me and we'll sort that out (local install vs Docker vs a free hosted instance like Neon)
before moving to migrations.

Better: don't put the real password in `appsettings.json` at all. Use user secrets instead:

```powershell
cd src\MaintenancePortal.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=maintenance_portal;Username=postgres;Password=YOUR_REAL_PASSWORD"
dotnet user-secrets set "Jwt:SigningKey" "some-long-random-string-at-least-32-chars"
```

User secrets override `appsettings.json` automatically in Development and never get
committed to git (they live outside the repo folder).

## 6. First migration (once packages + connection string are in place)

```powershell
cd src\MaintenancePortal.Api
dotnet ef migrations add InitialCreate --project ..\MaintenancePortal.Infrastructure
dotnet ef database update --project ..\MaintenancePortal.Infrastructure
```

## 7. Run it

```powershell
dotnet run --project src\MaintenancePortal.Api
```

Swagger UI should open at `http://localhost:5080/swagger`. Try `POST /api/auth/register`
with a body like:

```json
{
  "email": "landlord1@example.com",
  "password": "Password123",
  "fullName": "Alice Landlord",
  "role": "Landlord"
}
```

You should get back a JWT. Send me what you see (success or error) and we'll move to the
next phase (Property/Unit/TenantAssignment endpoints).
