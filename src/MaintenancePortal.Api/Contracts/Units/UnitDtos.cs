using System.ComponentModel.DataAnnotations;

namespace MaintenancePortal.Api.Contracts.Units;

public class CreateUnitRequest
{
	[Required, MaxLength(100)]
	public string UnitLabel { get; set; } = string.Empty;
}

public class UnitResponse
{
	public Guid Id { get; set; }
	public Guid PropertyId { get; set; }
	public string UnitLabel { get; set; } = string.Empty;
}