using MailKit.Net.Smtp;
using MimeKit;
using BookToAudio.Core.Config;
using BookToAudio.Core.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace BookToAudio.Infra.Services;

public sealed class EmailService : IEmailService
{
    private readonly EmailConfig _emailConfig;

    public EmailService(IOptions<EmailConfig> emailConfig)
    {
        _emailConfig = emailConfig.Value;
    }

    public void SendEmail(Core.Dto.EmailRequest request)
    {
        var email = new MimeMessage();  
        email.From.Add(new MailboxAddress(request.Name, _emailConfig.EmailFrom));
        email.To.Add(new MailboxAddress("Admin", _emailConfig.EmailTo));

        email.Subject = "BookToAudio";
        email.Body = new TextPart(MimeKit.Text.TextFormat.Text)
        {
            Text = $"Message from {request.UserEmail}:\n{request.Message}"
        };

        using var smtp = new SmtpClient();
        
        smtp.Connect("smtp.gmail.com", 587, false);
        smtp.Authenticate(_emailConfig.EmailFrom, _emailConfig.EmailFromPassword);
        smtp.Send(email);
        smtp.Disconnect(true);
    }
}