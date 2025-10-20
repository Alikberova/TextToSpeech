using TextToSpeech.Core.Dto;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra.Services;

public class EmailServiceStub : IEmailService
{
    public void SendEmail(EmailRequest request)
    {
    }
}
