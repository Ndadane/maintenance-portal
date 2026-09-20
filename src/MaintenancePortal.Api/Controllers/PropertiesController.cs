using MaintenancePortal.Api.Common;
using MaintenancePortal.Api.Contracts.Properties;
using MaintenancePortal.Core.Constants;
using MaintenancePortal.Core.Entities;
using MaintenancePortal.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintenancePortal.Api.Controllers;

[ApiController]
[Route("api/properties")]
[Authorize(Roles = Roles.Landlord)]
public class PropertiesController : ControllerBase
{
	private readonly AppDbContext _db;

	public PropertiesController(AppDbContext db)
	{
		_db = db;
	}

	[HttpPost]
	public async Task<ActionResult<PropertyResponse>> Create(CreatePropertyRequest request)
	{
		var landlordId = User.GetUserId();

		var property = new Property
		{
			Id = Guid.NewGuid(),
			LandlordId = landlordId,
			Name = request.Name,
			Address = request.Address
		};

		_db.Properties.Add(property);
		await _db.SaveChangesAsync();

		return StatusCode(StatusCodes.Status201Created, ToResponse(property));
	}

	[HttpGet]
	public async Task<ActionResult<List<PropertyResponse>>> GetAll()
	{
		var landlordId = User.GetUserId();

		// Query-level authorization: this filter is the actual security
		// boundary, not just a convenience - a landlord can only ever see
		// rows where they are the owner.
		var properties = await _db.Properties
			.Where(p => p.LandlordId == landlordId)
			.OrderBy(p => p.Name)
			.ToListAsync();

		return Ok(properties.Select(ToResponse));
	}

	private static PropertyResponse ToResponse(Property p) => new()
	{
		Id = p.Id,
		Name = p.Name,
		Address = p.Address
	};
}