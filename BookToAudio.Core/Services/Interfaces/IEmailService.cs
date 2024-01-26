namespace BookToAudio.Core.Services.Interfaces;

public interface IEmailService
{
  public  void FeedbackMessage(Dto.EmailRequest request);
}