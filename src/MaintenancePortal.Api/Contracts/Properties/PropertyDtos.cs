using System.ComponentModel.DataAnnotations;

namespace MaintenancePortal.Api.Contracts.Properties;

public class CreatePropertyRequest
{
	[Required, MaxLength(200)]
	public string Name { get; set; } = string.Empty;

	[Required, MaxLength(400)]
	public string Address { get; set; } = string.Empty;
}

public class PropertyResponse
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Address { get; set; } = string.Empty;
}