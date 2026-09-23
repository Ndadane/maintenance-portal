using MaintenancePortal.Api.Common;
using MaintenancePortal.Api.Contracts.MaintenanceRequests;
using MaintenancePortal.Core.Constants;
using MaintenancePortal.Core.Entities;
using MaintenancePortal.Core.Enums;
using MaintenancePortal.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintenancePortal.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class MaintenanceRequestsController : ControllerBase
{
	private readonly AppDbContext _db;

	public MaintenanceRequestsController(AppDbContext db)
	{
		_db = db;
	}

	// ---- GET /api/my-unit (Tenant only) ----
	[HttpGet("my-unit")]
	[Authorize(Roles = Roles.Tenant)]
	public async Task<ActionResult<MyUnitResponse>> GetMyUnit()
	{
		var tenantId = User.GetUserId();
		var today = DateOnly.FromDateTime(DateTime.UtcNow);

		// Plain property comparisons here (not TenantAssignment.IsActiveAsOf)
		// so EF Core can translate this straight to SQL - a C# instance
		// method call wouldn't translate.
		var assignment = await _db.TenantAssignments
			.Include(a => a.Unit)
			.ThenInclude(u => u!.Property)
			.Where(a => a.TenantId == tenantId
				&& a.MoveInDate <= today
				&& (a.MoveOutDate == null || a.MoveOutDate > today))
			.FirstOrDefaultAsync();

		if (assignment?.Unit?.Property is null)
		{
			return NotFound("No active unit assignment found.");
		}

		return Ok(new MyUnitResponse
		{
			AssignmentId = assignment.Id,
			UnitId = assignment.Unit.Id,
			UnitLabel = assignment.Unit.UnitLabel,
			PropertyId = assignment.Unit.Property.Id,
			PropertyName = assignment.Unit.Property.Name,
			PropertyAddress = assignment.Unit.Property.Address,
			MoveInDate = assignment.MoveInDate
		});
	}

	// ---- POST /api/requests (Tenant only) ----
	[HttpPost("requests")]
	[Authorize(Roles = Roles.Tenant)]
	public async Task<ActionResult<MaintenanceRequestResponse>> Create(CreateMaintenanceRequestRequest request)
	{
		var tenantId = User.GetUserId();
		var today = DateOnly.FromDateTime(DateTime.UtcNow);

		// The request always attaches to the tenant's CURRENT active
		// assignment - never a route/body parameter the tenant could
		// tamper with to attach a request to someone else's assignment.
		var activeAssignment = await _db.TenantAssignments
			.Include(a => a.Unit)
			.ThenInclude(u => u!.Property)
			.Where(a => a.TenantId == tenantId
				&& a.MoveInDate <= today
				&& (a.MoveOutDate == null || a.MoveOutDate > today))
			.FirstOrDefaultAsync();

		if (activeAssignment is null)
		{
			return BadRequest("You have no active unit assignment to submit a request for.");
		}

		var maintenanceRequest = new MaintenanceRequest
		{
			Id = Guid.NewGuid(),
			TenantAssignmentId = activeAssignment.Id,
			Title = request.Title,
			Description = request.Description,
			Category = request.Category,
			Priority = request.Priority,
			Status = RequestStatus.Submitted,
			CreatedAt = DateTimeOffset.UtcNow
		};

		_db.MaintenanceRequests.Add(maintenanceRequest);
		await _db.SaveChangesAsync();

		return StatusCode(StatusCodes.Status201Created, ToResponse(maintenanceRequest, activeAssignment.Unit!));
	}

	// ---- GET /api/requests (Tenant: own active assignment. Landlord: all owned properties, optional ?propertyId=) ----
	[HttpGet("requests")]
	public async Task<ActionResult<List<MaintenanceRequestResponse>>> GetAll([FromQuery] Guid? propertyId)
	{
		var userId = User.GetUserId();
		var isLandlord = User.IsInRole(Roles.Landlord);

		IQueryable<MaintenanceRequest> query = _db.MaintenanceRequests
			.Include(r => r.TenantAssignment)
			.ThenInclude(a => a!.Unit)
			.ThenInclude(u => u!.Property);

		if (isLandlord)
		{
			// Query-level authorization: only requests under properties this
			// landlord owns, ever - not filtered client-side.
			query = query.Where(r => r.TenantAssignment!.Unit!.Property!.LandlordId == userId);

			if (propertyId is not null)
			{
				query = query.Where(r => r.TenantAssignment!.Unit!.PropertyId == propertyId);
			}
		}
		else
		{
			// Tenant scope: ONLY their active assignment, never their
			// "current unit" as a whole - this is what stops a new tenant
			// from seeing the previous tenant's request history (handoff
			// doc section 26-27).
			var today = DateOnly.FromDateTime(DateTime.UtcNow);

			query = query.Where(r =>
				r.TenantAssignment!.TenantId == userId
				&& r.TenantAssignment.MoveInDate <= today
				&& (r.TenantAssignment.MoveOutDate == null || r.TenantAssignment.MoveOutDate > today));
		}

		var requests = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

		return Ok(requests.Select(r => ToResponse(r, r.TenantAssignment!.Unit!)));
	}

	// ---- GET /api/requests/{id} (Shared - resource-based check) ----
	[HttpGet("requests/{id:guid}")]
	public async Task<ActionResult<MaintenanceRequestResponse>> GetById(Guid id)
	{
		var userId = User.GetUserId();
		var isLandlord = User.IsInRole(Roles.Landlord);

		var maintenanceRequest = await _db.MaintenanceRequests
			.Include(r => r.TenantAssignment)
			.ThenInclude(a => a!.Unit)
			.ThenInclude(u => u!.Property)
			.FirstOrDefaultAsync(r => r.Id == id);

		if (maintenanceRequest is null)
		{
			return NotFound();
		}

		var authorized = IsAuthorized(maintenanceRequest, userId, isLandlord);

		// 404, not 403, for unauthorized access - avoids confirming the
		// resource exists to someone who shouldn't see it (handoff doc
		// section 33).
		if (!authorized)
		{
			return NotFound();
		}

		return Ok(ToResponse(maintenanceRequest, maintenanceRequest.TenantAssignment!.Unit!));
	}

	// ---- PATCH /api/requests/{id}/status (Landlord only) ----
	[HttpPatch("requests/{id:guid}/status")]
	[Authorize(Roles = Roles.Landlord)]
	public async Task<ActionResult<MaintenanceRequestResponse>> UpdateStatus(Guid id, UpdateMaintenanceRequestRequest request)
	{
		var landlordId = User.GetUserId();

		var maintenanceRequest = await _db.MaintenanceRequests
			.Include(r => r.TenantAssignment)
			.ThenInclude(a => a!.Unit)
			.ThenInclude(u => u!.Property)
			.FirstOrDefaultAsync(r => r.Id == id);

		if (maintenanceRequest?.TenantAssignment?.Unit?.Property is null
			|| maintenanceRequest.TenantAssignment.Unit.Property.LandlordId != landlordId)
		{
			return NotFound();
		}

		if (request.Status is null && request.Priority is null)
		{
			return BadRequest("Provide at least one of Status or Priority to update.");
		}

		if (request.Status is not null)
		{
			maintenanceRequest.Status = request.Status.Value;
			maintenanceRequest.ResolvedAt = request.Status.Value is RequestStatus.Resolved or RequestStatus.Closed
				? DateTimeOffset.UtcNow
				: null;
		}

		if (request.Priority is not null)
		{
			maintenanceRequest.Priority = request.Priority.Value;
		}

		await _db.SaveChangesAsync();

		return Ok(ToResponse(maintenanceRequest, maintenanceRequest.TenantAssignment.Unit));
	}

	internal static bool IsAuthorized(MaintenanceRequest request, Guid userId, bool isLandlord)
	{
		if (isLandlord)
		{
			return request.TenantAssignment?.Unit?.Property?.LandlordId == userId;
		}

		// Tenant: must be THIS request's assignment owner AND that
		// assignment must currently be active - a moved-out tenant loses
		// access even to their own former requests (handoff doc section 27,
		// 44).
		var today = DateOnly.FromDateTime(DateTime.UtcNow);
		var assignment = request.TenantAssignment;

		return assignment is not null
			&& assignment.TenantId == userId
			&& assignment.MoveInDate <= today
			&& (assignment.MoveOutDate is null || assignment.MoveOutDate > today);
	}

	private static MaintenanceRequestResponse ToResponse(MaintenanceRequest r, Core.Entities.Unit unit) => new()
	{
		Id = r.Id,
		Title = r.Title,
		Description = r.Description,
		Category = r.Category,
		Priority = r.Priority,
		Status = r.Status,
		CreatedAt = r.CreatedAt,
		ResolvedAt = r.ResolvedAt,
		UnitId = unit.Id,
		UnitLabel = unit.UnitLabel,
		PropertyId = unit.PropertyId,
		PropertyName = unit.Property?.Name ?? string.Empty
	};
}
