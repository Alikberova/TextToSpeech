using MailKit.Net.Smtp;
using MimeKit;
using BookToAudio.Core.Config;
using BookToAudio.Core.Services.Interfaces;


namespace BookToAudio.Services;

public class EmailService: IEmailService
{
    public void FeedbackMessage(Core.Dto.FeedbackRequest request)
    {
        var email = new MimeMessage();  
        email.From.Add(new MailboxAddress("User", EmailConfig.EmailFrom));
        email.To.Add(new MailboxAddress("Admin", EmailConfig.EmailTo));

        email.Subject = request.UserEmail;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Text)
        {
            Text = $"Message from {request.Name}:\n {request.Message}"
        };

        using (var smtp = new SmtpClient())
        {
            smtp.Connect("smtp.gmail.com", 587, false);
            smtp.Authenticate("ukr.bit.2023", EmailConfig.EmailFromPassword);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }

}
