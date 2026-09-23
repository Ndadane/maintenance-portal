namespace MaintenancePortal.Infrastructure.Storage;

public class StorageOptions
{
	public string ServiceUrl { get; set; } = "";
	public string Region { get; set; } = "";
	public string AccessKeyId { get; set; } = "";
	public string SecretAccessKey { get; set; } = "";
	public string BucketName { get; set; } = "";
}