namespace MaintenancePortal.Core.Abstractions;

public record StoredObjectInfo(long SizeBytes, string? ContentType);

public interface IFileStorageService
{
	/// <summary>Creates a temporary URL the client can PUT a file to.</summary>
	string CreateUploadUrl(string key, string contentType, TimeSpan validFor);

	/// <summary>Creates a temporary URL the client can GET a file from.</summary>
	string CreateDownloadUrl(string key, TimeSpan validFor);

	/// <summary>Returns size/type of a stored file, or null if it doesn't exist.</summary>
	Task<StoredObjectInfo?> GetObjectInfoAsync(string key, CancellationToken ct = default);

	Task DeleteAsync(string key, CancellationToken ct = default);
}