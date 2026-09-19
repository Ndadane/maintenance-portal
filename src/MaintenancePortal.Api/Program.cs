using System.Text;
using MaintenancePortal.Core.Abstractions;
using MaintenancePortal.Core.Constants;
using MaintenancePortal.Infrastructure.Auth;
using MaintenancePortal.Infrastructure.Data;
using MaintenancePortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---- EF Core / PostgreSQL ----
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(
		builder.Configuration.GetConnectionString("Default"),
		npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
			maxRetryCount: 5,
			maxRetryDelay: TimeSpan.FromSeconds(10),
			errorCodesToAdd: null)));

// ---- ASP.NET Core Identity ----
// AddIdentity (not AddIdentityCore) because we need the full sign-in stack
// (SignInManager, cookie/token integration) plus role management.
builder.Services
	.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
	{
		// Reasonable MVP defaults - tightened later if needed.
		options.Password.RequiredLength = 8;
		options.Password.RequireNonAlphanumeric = false;
		options.User.RequireUniqueEmail = true;
	})
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();

// ---- JWT settings (Options pattern) ----
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
	?? throw new InvalidOperationException("Jwt configuration section is missing.");

// ---- Authentication: JWT Bearer ----
builder.Services
	.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = jwtSettings.Issuer,

			ValidateAudience = true,
			ValidAudience = jwtSettings.Audience,

			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),

			ValidateLifetime = true,
			ClockSkew = TimeSpan.FromMinutes(1)
		};
	});

// Authorization services. Resource-based authorization handlers (for
// per-request checks like "can this tenant see request 123") get registered
// here once implemented in Phase 9 - not part of the initial scaffold.
builder.Services.AddAuthorization();

// ---- App services ----
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---- Seed the two fixed roles on startup ----
// Landlord/Tenant are a fixed, small set (not user-manageable data), so
// seeding on startup is simpler than a migration-based data seed here.
using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
	foreach (var role in new[] { Roles.Landlord, Roles.Tenant })
	{
		if (!await roleManager.RoleExistsAsync(role))
		{
			await roleManager.CreateAsync(new IdentityRole<Guid>(role));
		}
	}
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Order matters: Authentication must run before Authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory-based integration tests (Phase 3/11).
public partial class Program { }
