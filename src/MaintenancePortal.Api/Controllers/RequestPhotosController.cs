using MaintenancePortal.Api.Common;
using MaintenancePortal.Api.Contracts.MaintenanceRequests;
using MaintenancePortal.Core.Abstractions;
using MaintenancePortal.Core.Constants;
using MaintenancePortal.Core.Entities;
using MaintenancePortal.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintenancePortal.Api.Controllers;

[ApiController]
[Route("api/requests/{requestId:guid}/photos")]
[Authorize]
public class RequestPhotosController : ControllerBase
{
	private const long MaxPhotoBytes = 10 * 1024 * 1024; // 10 MB
	private const int MaxPhotosPerRequest = 10;
	private static readonly TimeSpan UploadLinkLifetime = TimeSpan.FromMinutes(10);
	private static readonly TimeSpan DownloadLinkLifetime = TimeSpan.FromMinutes(15);

	private static readonly Dictionary<string, string> AllowedTypes =
		new(StringComparer.OrdinalIgnoreCase)
		{
			["image/jpeg"] = ".jpg",
			["image/png"] = ".png",
			["image/webp"] = ".webp"
		};

	private readonly AppDbContext _db;
	private readonly IFileStorageService _storage;

	public RequestPhotosController(AppDbContext db, IFileStorageService storage)
	{
		_db = db;
		_storage = storage;
	}

	// ---- POST /api/requests/{requestId}/photos/presign ----
	[HttpPost("presign")]
	public async Task<ActionResult<PresignPhotoResponse>> Presign(Guid requestId, PresignPhotoRequest body)
	{
		var request = await LoadAuthorizedRequestAsync(requestId);
		if (request is null)
		{
			return NotFound();
		}

		if (!AllowedTypes.TryGetValue(body.ContentType, out var extension))
		{
			return BadRequest("Only JPEG, PNG and WEBP images are allowed.");
		}

		if (body.SizeBytes <= 0 || body.SizeBytes > MaxPhotoBytes)
		{
			return BadRequest("Photo must be larger than 0 bytes and at most 10 MB.");
		}

		var count = await _db.RequestPhotos.CountAsync(p => p.RequestId == requestId);
		if (count >= MaxPhotosPerRequest)
		{
			return BadRequest($"A request can have at most {MaxPhotosPerRequest} photos.");
		}

		var contentType = body.ContentType.ToLowerInvariant();
		var key = $"requests/{requestId}/{Guid.NewGuid()}{extension}";
		var url = _storage.CreateUploadUrl(key, contentType, UploadLinkLifetime);

		return Ok(new PresignPhotoResponse
		{
			Key = key,
			UploadUrl = url,
			ContentType = contentType,
			ExpiresInSeconds = (int)UploadLinkLifetime.TotalSeconds
		});
	}

	// ---- POST /api/requests/{requestId}/photos/confirm ----
	[HttpPost("confirm")]
	public async Task<ActionResult<PhotoResponse>> Confirm(
		Guid requestId, ConfirmPhotoRequest body, CancellationToken ct)
	{
		var request = await LoadAuthorizedRequestAsync(requestId);
		if (request is null)
		{
			return NotFound();
		}

		// The key must belong to THIS request, so nobody can attach
		// another request's file to their own request.
		var prefix = $"requests/{requestId}/";
		if (!body.Key.StartsWith(prefix, StringComparison.Ordinal) || body.Key.Contains(".."))
		{
			return BadRequest("Invalid photo key.");
		}

		if (await _db.RequestPhotos.AnyAsync(p => p.StorageKey == body.Key, ct))
		{
			return Conflict("This photo has already been confirmed.");
		}

		// Ask storage what actually arrived; never trust what the client claims.
		var info = await _storage.GetObjectInfoAsync(body.Key, ct);
		if (info is null)
		{
			return BadRequest("File not found in storage. Upload it first, then confirm.");
		}

		var contentType = (info.ContentType ?? string.Empty).ToLowerInvariant();
		if (info.SizeBytes <= 0 || info.SizeBytes > MaxPhotoBytes || !AllowedTypes.ContainsKey(contentType))
		{
			await _storage.DeleteAsync(body.Key, ct);
			return BadRequest("Uploaded file was rejected (wrong type or larger than 10 MB) and has been deleted.");
		}

		var photo = new RequestPhoto
		{
			Id = Guid.NewGuid(),
			RequestId = requestId,
			StorageKey = body.Key,
			ContentType = contentType,
			SizeBytes = info.SizeBytes,
			UploadedAt = DateTimeOffset.UtcNow
		};

		_db.RequestPhotos.Add(photo);
		await _db.SaveChangesAsync(ct);

		return StatusCode(StatusCodes.Status201Created, ToResponse(photo));
	}

	// ---- GET /api/requests/{requestId}/photos ----
	[HttpGet]
	public async Task<ActionResult<List<PhotoResponse>>> List(Guid requestId, CancellationToken ct)
	{
		var request = await LoadAuthorizedRequestAsync(requestId);
		if (request is null)
		{
			return NotFound();
		}

		var photos = await _db.RequestPhotos
			.Where(p => p.RequestId == requestId)
			.OrderBy(p => p.UploadedAt)
			.ToListAsync(ct);

		return Ok(photos.Select(ToResponse).ToList());
	}

	// Same ownership rules as GET /api/requests/{id}; returns null (-> 404)
	// if the request doesn't exist OR the caller isn't allowed to see it.
	private async Task<MaintenanceRequest?> LoadAuthorizedRequestAsync(Guid requestId)
	{
		var request = await _db.MaintenanceRequests
			.Include(r => r.TenantAssignment)
			.ThenInclude(a => a!.Unit)
			.ThenInclude(u => u!.Property)
			.FirstOrDefaultAsync(r => r.Id == requestId);

		if (request is null)
		{
			return null;
		}

		var authorized = MaintenanceRequestsController.IsAuthorized(
			request, User.GetUserId(), User.IsInRole(Roles.Landlord));

		return authorized ? request : null;
	}

	private PhotoResponse ToResponse(RequestPhoto p) => new()
	{
		Id = p.Id,
		ContentType = p.ContentType,
		SizeBytes = p.SizeBytes,
		UploadedAt = p.UploadedAt,
		DownloadUrl = _storage.CreateDownloadUrl(p.StorageKey, DownloadLinkLifetime)
	};
}