using MaintenancePortal.Api.Common;
using MaintenancePortal.Api.Contracts.MaintenanceRequests;
using MaintenancePortal.Core.Constants;
using MaintenancePortal.Core.Entities;
using MaintenancePortal.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintenancePortal.Api.Controllers;

[ApiController]
[Route("api/requests/{requestId:guid}/comments")]
[Authorize]
public class RequestCommentsController : ControllerBase
{
	private readonly AppDbContext _db;

	public RequestCommentsController(AppDbContext db)
	{
		_db = db;
	}

	// ---- POST /api/requests/{requestId}/comments ----
	[HttpPost]
	public async Task<ActionResult<CommentResponse>> Create(
		Guid requestId, CreateCommentRequest body, CancellationToken ct)
	{
		var request = await LoadAuthorizedRequestAsync(requestId, ct);
		if (request is null)
		{
			return NotFound();
		}

		var text = body.Body.Trim();
		if (text.Length == 0)
		{
			return BadRequest("Comment cannot be empty.");
		}

		var comment = new RequestComment
		{
			Id = Guid.NewGuid(),
			RequestId = requestId,
			AuthorId = User.GetUserId(),
			Body = text,
			CreatedAt = DateTimeOffset.UtcNow
		};

		_db.RequestComments.Add(comment);
		await _db.SaveChangesAsync(ct);

		var responses = await ToResponsesAsync(request, new List<RequestComment> { comment }, ct);
		return StatusCode(StatusCodes.Status201Created, responses[0]);
	}

	// ---- GET /api/requests/{requestId}/comments ----
	[HttpGet]
	public async Task<ActionResult<List<CommentResponse>>> List(Guid requestId, CancellationToken ct)
	{
		var request = await LoadAuthorizedRequestAsync(requestId, ct);
		if (request is null)
		{
			return NotFound();
		}

		var comments = await _db.RequestComments
			.Where(c => c.RequestId == requestId)
			.OrderBy(c => c.CreatedAt)
			.ToListAsync(ct);

		return Ok(await ToResponsesAsync(request, comments, ct));
	}

	// Same ownership rules as GET /api/requests/{id}; null => 404.
	private async Task<MaintenanceRequest?> LoadAuthorizedRequestAsync(Guid requestId, CancellationToken ct)
	{
		var request = await _db.MaintenanceRequests
			.Include(r => r.TenantAssignment)
			.ThenInclude(a => a!.Unit)
			.ThenInclude(u => u!.Property)
			.FirstOrDefaultAsync(r => r.Id == requestId, ct);

		if (request is null)
		{
			return null;
		}

		var authorized = MaintenanceRequestsController.IsAuthorized(
			request, User.GetUserId(), User.IsInRole(Roles.Landlord));

		return authorized ? request : null;
	}

	private async Task<List<CommentResponse>> ToResponsesAsync(
		MaintenanceRequest request, List<RequestComment> comments, CancellationToken ct)
	{
		var landlordId = request.TenantAssignment!.Unit!.Property!.LandlordId;
		var authorIds = comments.Select(c => c.AuthorId).Distinct().ToList();

		var emails = await _db.Users
			.Where(u => authorIds.Contains(u.Id))
			.ToDictionaryAsync(u => u.Id, u => u.Email ?? string.Empty, ct);

		return comments.Select(c => new CommentResponse
		{
			Id = c.Id,
			AuthorId = c.AuthorId,
			AuthorEmail = emails.GetValueOrDefault(c.AuthorId, string.Empty),
			AuthorRole = c.AuthorId == landlordId ? Roles.Landlord : Roles.Tenant,
			Body = c.Body,
			CreatedAt = c.CreatedAt
		}).ToList();
	}
}