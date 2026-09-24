using MailKit.Net.Smtp;
using MailKit.Security;
using MaintenancePortal.Core.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MaintenancePortal.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
	private readonly EmailOptions _o;

	public SmtpEmailSender(IOptions<EmailOptions> options)
	{
		_o = options.Value;
	}

	public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
	{
		var mime = new MimeMessage();
		mime.From.Add(new MailboxAddress(_o.FromName, _o.FromEmail));
		mime.To.Add(MailboxAddress.Parse(message.To));
		mime.Subject = message.Subject;
		mime.Body = new TextPart("plain") { Text = message.Body };

		using var client = new SmtpClient();
		await client.ConnectAsync(_o.Host, _o.Port, SecureSocketOptions.StartTls, ct);
		await client.AuthenticateAsync(_o.Username, _o.Password, ct);
		await client.SendAsync(mime, ct);
		await client.DisconnectAsync(true, ct);
	}
}
