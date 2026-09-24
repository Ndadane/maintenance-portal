using MaintenancePortal.Core.Abstractions;
using MaintenancePortal.Infrastructure.Email;

namespace MaintenancePortal.Api.BackgroundServices;

public class EmailBackgroundService : BackgroundService
{
	private readonly ChannelEmailQueue _queue;
	private readonly IEmailSender _sender;
	private readonly ILogger<EmailBackgroundService> _logger;

	public EmailBackgroundService(
		ChannelEmailQueue queue, IEmailSender sender, ILogger<EmailBackgroundService> logger)
	{
		_queue = queue;
		_sender = sender;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await foreach (var message in _queue.ReadAllAsync(stoppingToken))
		{
			try
			{
				await _sender.SendAsync(message, stoppingToken);
				_logger.LogInformation("Email sent to {To}: {Subject}", message.To, message.Subject);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send email to {To}", message.To);
			}
		}
	}
}
