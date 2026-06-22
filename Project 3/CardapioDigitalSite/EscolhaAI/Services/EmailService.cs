using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;

namespace EscolhaAI.Services;
public class EmailService
{
    public bool SendEmail(
        string subject,
        string body,
        string emailFrom,
        string emailTo,
        List<string>? emailBccList)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(emailFrom));
        message.To.Add(MailboxAddress.Parse(emailTo));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();

#if DEBUG
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
#endif

        // 🔥 ESSENCIAL NA KINGHOST
        client.Connect("mail.escolha.ai", 465, SecureSocketOptions.SslOnConnect);

        // garante que AUTH está disponível
        if (!client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
            throw new Exception("Servidor SMTP não suporta autenticação.");

        client.Authenticate(
            "escolha@escolha.ai",
            "NINAACLSDd201@12" // senha do WEBMAIL
        );

        client.Send(message);
        client.Disconnect(true);

        return true;
    }
}
