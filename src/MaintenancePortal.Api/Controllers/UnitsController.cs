using MaintenancePortal.Api.Common;
using MaintenancePortal.Api.Contracts.TenantAssignments;
using MaintenancePortal.Api.Contracts.Units;
using MaintenancePortal.Core.Constants;
using MaintenancePortal.Core.Entities;
using MaintenancePortal.Infrastructure.Data;
using MaintenancePortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintenancePortal.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = Roles.Landlord)]
public class UnitsController : ControllerBase
{
	private readonly AppDbContext _db;
	private readonly UserManager<ApplicationUser> _userManager;

	public UnitsController(AppDbContext db, UserManager<ApplicationUser> userManager)
	{
		_db = db;
		_userManager = userManager;
	}

	[HttpPost("properties/{propertyId:guid}/units")]
	public async Task<ActionResult<UnitResponse>> CreateUnit(Guid propertyId, CreateUnitRequest request)
	{
		var landlordId = User.GetUserId();

		var property = await _db.Properties.FirstOrDefaultAsync(p => p.Id == propertyId);

		// 404 rather than 403 for a property that exists but isn't theirs -
		// matches the project's stated preference to avoid resource-ID
		// enumeration (see handoff doc section 33).
		if (property is null || property.LandlordId != landlordId)
		{
			return NotFound();
		}

		var unit = new Unit
		{
			Id = Guid.NewGuid(),
			PropertyId = propertyId,
			UnitLabel = request.UnitLabel
		};

		_db.Units.Add(unit);
		await _db.SaveChangesAsync();

		return StatusCode(StatusCodes.Status201Created, ToResponse(unit));
	}

	// Not explicitly listed in the original endpoint spec, but "CRUD" implies
	// read too, and the frontend will need a way to list a property's units.
	// Minor implementation decision - flagging it rather than treating it as
	// silently obvious.
	[HttpGet("properties/{propertyId:guid}/units")]
	public async Task<ActionResult<List<UnitResponse>>> GetUnits(Guid propertyId)
	{
		var landlordId = User.GetUserId();

		var property = await _db.Properties.FirstOrDefaultAsync(p => p.Id == propertyId);
		if (property is null || property.LandlordId != landlordId)
		{
			return NotFound();
		}

		var units = await _db.Units
			.Where(u => u.PropertyId == propertyId)
			.OrderBy(u => u.UnitLabel)
			.ToListAsync();

		return Ok(units.Select(ToResponse));
	}

	[HttpPost("units/{unitId:guid}/tenants")]
	public async Task<ActionResult<TenantAssignmentResponse>> AssignTenant(Guid unitId, AssignTenantRequest request)
	{
		var landlordId = User.GetUserId();

		var unit = await _db.Units
			.Include(u => u.Property)
			.FirstOrDefaultAsync(u => u.Id == unitId);

		if (unit is null || unit.Property is null || unit.Property.LandlordId != landlordId)
		{
			return NotFound();
		}

		if (request.MoveOutDate is not null && request.MoveOutDate <= request.MoveInDate)
		{
			return BadRequest("MoveOutDate must be after MoveInDate.");
		}

		// Application-level overlap check (see handoff doc section 16) -
		// no database-level constraint yet, that's a documented future
		// enhancement.
		var existingAssignments = await _db.TenantAssignments
			.Where(a => a.UnitId == unitId)
			.ToListAsync();

		if (existingAssignments.Any(a => a.OverlapsWith(request.MoveInDate, request.MoveOutDate)))
		{
			return Conflict("This date range overlaps an existing tenant assignment for this unit.");
		}

		var tenantUser = await _userManager.FindByEmailAsync(request.TenantEmail);
		var tenantWasAutoCreated = false;
		string? temporaryPassword = null;

		if (tenantUser is null)
		{
			// Decision: auto-create a placeholder Tenant account rather than
			// rejecting the assignment. No invite email is sent yet (that's
			// Phase 6) - the landlord must pass the temporary password to
			// the tenant themselves for now.
			temporaryPassword = GenerateTemporaryPassword();

			tenantUser = new ApplicationUser
			{
				UserName = request.TenantEmail,
				Email = request.TenantEmail,
				FullName = request.TenantEmail // placeholder - tenant can update this after first login
			};

			var createResult = await _userManager.CreateAsync(tenantUser, temporaryPassword);
			if (!createResult.Succeeded)
			{
				return BadRequest(createResult.Errors.Select(e => e.Description));
			}

			await _userManager.AddToRoleAsync(tenantUser, Roles.Tenant);
			tenantWasAutoCreated = true;
		}
		else
		{
			var roles = await _userManager.GetRolesAsync(tenantUser);
			if (!roles.Contains(Roles.Tenant))
			{
				return BadRequest("This email belongs to an existing account that is not a Tenant.");
			}
		}

		var assignment = new TenantAssignment
		{
			Id = Guid.NewGuid(),
			TenantId = tenantUser.Id,
			UnitId = unitId,
			MoveInDate = request.MoveInDate,
			MoveOutDate = request.MoveOutDate
		};

		_db.TenantAssignments.Add(assignment);
		await _db.SaveChangesAsync();

		return StatusCode(StatusCodes.Status201Created, new TenantAssignmentResponse
		{
			Id = assignment.Id,
			UnitId = unitId,
			TenantId = tenantUser.Id,
			TenantEmail = request.TenantEmail,
			MoveInDate = assignment.MoveInDate,
			MoveOutDate = assignment.MoveOutDate,
			TenantWasAutoCreated = tenantWasAutoCreated,
			TemporaryPassword = temporaryPassword
		});
	}

	private static string GenerateTemporaryPassword()
	{
		// Satisfies Identity's configured password policy (>= 8 chars,
		// mixed case, includes a digit and symbol) without needing the
		// tenant to have set anything themselves yet.
		return $"Tmp{Guid.NewGuid():N}"[..14] + "9!";
	}

	private static UnitResponse ToResponse(Unit u) => new()
	{
		Id = u.Id,
		PropertyId = u.PropertyId,
		UnitLabel = u.UnitLabel
	};
}