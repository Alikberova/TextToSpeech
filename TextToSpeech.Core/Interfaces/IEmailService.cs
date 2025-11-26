namespace TextToSpeech.Core.Interfaces;

public interface IEmailService
{
    public void SendEmail(Dto.EmailRequest request);
}