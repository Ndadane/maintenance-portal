using System.ComponentModel.DataAnnotations;

namespace MaintenancePortal.Api.Contracts.MaintenanceRequests;

public class PresignPhotoRequest
{
	[Required]
	public string ContentType { get; set; } = string.Empty;

	public long SizeBytes { get; set; }
}

public class PresignPhotoResponse
{
	public string Key { get; set; } = string.Empty;
	public string UploadUrl { get; set; } = string.Empty;
	public string ContentType { get; set; } = string.Empty;
	public int ExpiresInSeconds { get; set; }
}

public class ConfirmPhotoRequest
{
	[Required]
	public string Key { get; set; } = string.Empty;
}

public class PhotoResponse
{
	public Guid Id { get; set; }
	public string ContentType { get; set; } = string.Empty;
	public long SizeBytes { get; set; }
	public DateTimeOffset UploadedAt { get; set; }
	public string DownloadUrl { get; set; } = string.Empty;
}