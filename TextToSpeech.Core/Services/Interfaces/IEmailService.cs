namespace TextToSpeech.Core.Services.Interfaces;

public interface IEmailService
{
  public  void SendEmail(Dto.EmailRequest request);
}