using System.Threading.Channels;
using MaintenancePortal.Core.Abstractions;

namespace MaintenancePortal.Infrastructure.Email;

public class ChannelEmailQueue : IEmailQueue
{
	private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(
		new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.DropOldest });

	public ValueTask EnqueueAsync(EmailMessage message, CancellationToken ct = default)
		=> _channel.Writer.WriteAsync(message, ct);

	public IAsyncEnumerable<EmailMessage> ReadAllAsync(CancellationToken ct)
		=> _channel.Reader.ReadAllAsync(ct);
}
