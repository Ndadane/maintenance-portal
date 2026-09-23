using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using MaintenancePortal.Core.Abstractions;
using Microsoft.Extensions.Options;

namespace MaintenancePortal.Infrastructure.Storage;

public class S3FileStorageService : IFileStorageService
{
	private readonly AmazonS3Client _client;
	private readonly string _bucket;

	public S3FileStorageService(IOptions<StorageOptions> options)
	{
		var o = options.Value;

		if (string.IsNullOrWhiteSpace(o.ServiceUrl) ||
			string.IsNullOrWhiteSpace(o.Region) ||
			string.IsNullOrWhiteSpace(o.AccessKeyId) ||
			string.IsNullOrWhiteSpace(o.SecretAccessKey) ||
			string.IsNullOrWhiteSpace(o.BucketName))
		{
			throw new InvalidOperationException(
				"Storage settings are missing. Check the Storage:* user-secrets.");
		}

		_bucket = o.BucketName;

		var config = new AmazonS3Config
		{
			ServiceURL = o.ServiceUrl,
			AuthenticationRegion = o.Region,
			ForcePathStyle = true,
			// Newer AWS SDKs add extra checksum headers by default; only send them
			// when required, since S3-compatible providers can reject them.
			RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
			ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
		};

		_client = new AmazonS3Client(
			new BasicAWSCredentials(o.AccessKeyId, o.SecretAccessKey), config);
	}

	public string CreateUploadUrl(string key, string contentType, TimeSpan validFor)
	{
		var request = new GetPreSignedUrlRequest
		{
			BucketName = _bucket,
			Key = key,
			Verb = HttpVerb.PUT,
			Expires = DateTime.UtcNow.Add(validFor),
			ContentType = contentType // signed into the URL, so the upload must match it
		};
		return _client.GetPreSignedURL(request);
	}

	public string CreateDownloadUrl(string key, TimeSpan validFor)
	{
		var request = new GetPreSignedUrlRequest
		{
			BucketName = _bucket,
			Key = key,
			Verb = HttpVerb.GET,
			Expires = DateTime.UtcNow.Add(validFor)
		};
		return _client.GetPreSignedURL(request);
	}

	public async Task<StoredObjectInfo?> GetObjectInfoAsync(string key, CancellationToken ct = default)
	{
		try
		{
			var response = await _client.GetObjectMetadataAsync(_bucket, key, ct);
			return new StoredObjectInfo(response.Headers.ContentLength, response.Headers.ContentType);
		}
		catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task DeleteAsync(string key, CancellationToken ct = default)
	{
		await _client.DeleteObjectAsync(_bucket, key, ct);
	}
}