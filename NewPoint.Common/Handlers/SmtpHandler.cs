using MimeKit;
using MailKit.Net.Smtp;
using NewPoint.Common.Configurations;

namespace NewPoint.Common.Handlers;

public static class SmtpHandler
{
    public static SmtpConfiguration Configuration { get; set; }

    public static async Task SendEmail(string email, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("NewPoint", Configuration.Email));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = body
        };
        using var client = new SmtpClient();
        await client.ConnectAsync(Configuration.Server, Configuration.Port, false);
        await client.AuthenticateAsync(Configuration.Email, Configuration.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}