namespace MaintenancePortal.Core.Abstractions;

public record EmailMessage(string To, string Subject, string Body);

public interface IEmailSender
{
	Task SendAsync(EmailMessage message, CancellationToken ct = default);
}

public interface IEmailQueue
{
	ValueTask EnqueueAsync(EmailMessage message, CancellationToken ct = default);
}
