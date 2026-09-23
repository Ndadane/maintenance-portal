namespace MaintenancePortal.Core.Entities;

/// <summary>
/// Metadata for a photo attached to a maintenance request. The actual image
/// bytes live in S3-compatible object storage (Cloudflare R2 or AWS S3) -
/// PostgreSQL only stores the resulting URL and upload time. Detailed secure
/// upload design (presigned vs API-mediated) is deferred to the photo
/// implementation phase.
/// </summary>

public class RequestPhoto
{
	public Guid Id { get; set; }

	public Guid RequestId { get; set; }
	public MaintenanceRequest? Request { get; set; }

	public string StorageKey { get; set; } = string.Empty;
	public string ContentType { get; set; } = string.Empty;
	public long SizeBytes { get; set; }

	public DateTimeOffset UploadedAt { get; set; }
}