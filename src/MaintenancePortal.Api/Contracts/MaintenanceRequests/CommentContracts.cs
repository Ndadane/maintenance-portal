using System.ComponentModel.DataAnnotations;

namespace MaintenancePortal.Api.Contracts.MaintenanceRequests;

public class CreateCommentRequest
{
	[Required]
	[MaxLength(2000)]
	public string Body { get; set; } = string.Empty;
}

public class CommentResponse
{
	public Guid Id { get; set; }
	public Guid AuthorId { get; set; }
	public string AuthorEmail { get; set; } = string.Empty;
	public string AuthorRole { get; set; } = string.Empty;
	public string Body { get; set; } = string.Empty;
	public DateTimeOffset CreatedAt { get; set; }
}
